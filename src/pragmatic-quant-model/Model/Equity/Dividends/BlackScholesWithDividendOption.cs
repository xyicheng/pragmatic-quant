using System;
using System.Diagnostics.Contracts;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Function;
using pragmatic_quant_model.Maths.Integration;

namespace pragmatic_quant_model.Model.Equity.Dividends
{
    /// <summary>
    /// Vanilla option pricer for Black-Scholes model with dividends.
    /// </summary>
    public class BlackScholesWithDividendOption
    {
        #region private fields
        private readonly double spot;
        private readonly AffineDivCurveUtils affineDivUtils;
        private readonly double[] quadPoints;
        private readonly double[] quadWeights;
        #endregion
        
        public BlackScholesWithDividendOption(double spot, AffineDivCurveUtils affineDivUtils, int quadratureNbPoints = 10)
        {
            this.affineDivUtils = affineDivUtils;
            this.spot = spot;
            GaussHermite.GetQuadrature(quadratureNbPoints, out quadPoints, out quadWeights);
        }

        public static BlackScholesWithDividendOption Build(double spot,
                                                           DividendQuote[] dividends,
                                                           DiscountCurve discountCurve,
                                                           ITimeMeasure time)
        {
            var divUtils = new AffineDivCurveUtils(dividends, discountCurve, time);
            return new BlackScholesWithDividendOption(spot, divUtils);
        }
        
        /// <summary>
        /// Price option using a two step quadrature.
        /// </summary>
        /// <param name="maturity">option maturity</param>
        /// <param name="strike">option strike</param>
        /// <param name="vol">volatility</param>
        /// <param name="q">option type : 1 for call, -1 for put</param>
        /// <returns></returns>
        public double Price(double maturity, double strike, double vol, double q)
        {
            return new BsDivPrice(maturity, strike, spot, affineDivUtils, quadPoints, quadWeights).Price(vol, q);
        }

        /// <summary>
        /// Price option using a two step quadrature.
        /// </summary>
        /// <param name="maturity">option maturity</param>
        /// <param name="strike">option strike</param>
        /// <param name="terminalVol"> terminal volatility : quadratic average of BS instant volatility </param>
        /// <param name="q">option type : 1 for call, -1 for put</param>
        /// <returns></returns>
        public double Price(double maturity, double strike, Func<double, double> terminalVol, double q)
        {
            return new BsDivPrice(maturity, strike, spot, affineDivUtils, quadPoints, quadWeights).Price(terminalVol, q);
        }

        /// <summary>
        /// Implied volatility from option price. (Inversion of the two step quadrature formula.)
        /// </summary>
        /// <param name="maturity">option maturity</param>
        /// <param name="strike">option strike</param>
        /// <param name="price">option price </param>
        /// <param name="q">option type : 1 for call, -1 for put</param>
        /// <returns></returns>
        public double ImpliedVol(double maturity, double strike, double price, double q)
        {
            //Proxy using Lehman Formula
            double proxyFwd, proxyDk;
            affineDivUtils.LehmanProxy(maturity, spot, out proxyFwd, out proxyDk);
            double lehmanTargetVol = BlackScholesOption.ImpliedVol(price, proxyFwd, strike + proxyDk, maturity, q);
            
            var pricer = new BsDivPrice(maturity, strike, spot, affineDivUtils, quadPoints, quadWeights);
            Func<double, double> volToLehmanVolErr =
                v => BlackScholesOption.ImpliedVol(pricer.Price(v, q), proxyFwd, strike + proxyDk, maturity, q) - lehmanTargetVol;
            //TODO use a cache

            //Bracket & Solve
            double v1 = lehmanTargetVol;
            double v2 = lehmanTargetVol - volToLehmanVolErr(lehmanTargetVol);
            if (!RootUtils.Bracket(volToLehmanVolErr, v1, v2, out v1, out v2))
                throw new Exception("Failed to inverse vol");
            var impliedVol = RootUtils.Brenth(volToLehmanVolErr, v1, v2, 1.0e-10, 2.0 * DoubleUtils.MachineEpsilon, 10);
            return impliedVol;
        }

        public double[] CalibrateVol(double[] maturities, double[] targetPrices, double[] strikes, double[] optionTypes)
        {
            Contract.Requires(EnumerableUtils.IsSorted(maturities));
            Contract.Requires(maturities.Length == strikes.Length 
                              && strikes.Length == targetPrices.Length
                              && targetPrices.Length== optionTypes.Length);
            
            double[] variances = new double[maturities.Length + 1];
            double[] varPillars = new[] {0.0}.Concat(maturities).ToArray();

            double[] calibVols = new double[maturities.Length];
            for (int step = 0; step < maturities.Length; step++)
            {
                var maturity = maturities[step];
                var strike = strikes[step];
                var targetPrice = targetPrices[step];
                var q = optionTypes[step];
                
                //Proxy using Lehman Formula
                double proxyFwd, proxyDk;
                affineDivUtils.LehmanProxy(maturity, spot, out proxyFwd, out proxyDk);
                double lehmanTargetVol = BlackScholesOption.ImpliedVol(targetPrice, proxyFwd, strike + proxyDk, maturity, q);

                var pricer = new BsDivPrice(maturities[step], strikes[step], spot, affineDivUtils, quadPoints, quadWeights);
                Func<double, double> volToLehmanVolErr = v =>
                {
                    variances[1 + step] = v * v * maturity;
                    var varFunc = RrFunctions.LinearInterpolation(varPillars, variances);
                    
                    Func<double, double> volFunc = t => Math.Sqrt(varFunc.Eval(t) / t);
                    var price = pricer.Price(volFunc, q);
                    return BlackScholesOption.ImpliedVol(price, proxyFwd, strike + proxyDk, maturity, q) - lehmanTargetVol;
                };//TODO use a cache
                
                
                //Bracket & Solve
                double v1 = lehmanTargetVol;
                double v2;
                if (step == 0)
                {
                    v2 = lehmanTargetVol - volToLehmanVolErr(lehmanTargetVol);
                }
                else
                {
                    var volIfZeroVolOnStep = Math.Sqrt(calibVols[step - 1] * calibVols[step - 1] * maturities[step - 1] / maturities[step]);
                    var minError = volToLehmanVolErr(volIfZeroVolOnStep);
                    if (minError > 0.0) //saturation case
                    {
                        calibVols[step] = volIfZeroVolOnStep;
                        variances[1 + step] = volIfZeroVolOnStep * volIfZeroVolOnStep * maturity;
                        continue;
                    }
                    v2 = volIfZeroVolOnStep;
                }
                
                if (!RootUtils.Bracket(volToLehmanVolErr, v1, v2, out v1, out v2))
                    throw new Exception("Failed to inverse vol");
                var impliedVol = RootUtils.Brenth(volToLehmanVolErr, v1, v2, 1.0e-10, 2.0 * DoubleUtils.MachineEpsilon, 10);
                
                calibVols[step] = impliedVol;
                variances[1 + step] = impliedVol * impliedVol * maturity;
            }

            return calibVols;
        }
        public double[] CalibrateVol(double[] maturities, double[] targetPrices, double strike, double optionType)
        {
            return CalibrateVol(maturities, targetPrices,
                ArrayUtils.Constant(maturities.Length, strike),
                ArrayUtils.Constant(maturities.Length, optionType));
        }
        
        /// <summary>
        /// Price option using Lehman Brother proxy.
        /// </summary>
        /// <param name="maturity">option maturity</param>
        /// <param name="strike">option strike</param>
        /// <param name="vol">volatility</param>
        /// <param name="q">option type : 1 for call, -1 for put</param>
        /// <returns></returns>
        public double PriceLehman(double maturity, double strike, double vol, double q)
        {
            double effectiveForward, strikeShift;
            affineDivUtils.LehmanProxy(maturity, spot, out effectiveForward, out strikeShift);
            return BlackScholesOption.Price(effectiveForward, strike + strikeShift, vol, maturity, q);
        }
        
        public double PriceProxy(double maturity, double strike, double vol, double q)
        {
            var growth = affineDivUtils.AssetGrowth(maturity);
            double cashBpv = affineDivUtils.CashDivBpv(maturity);
            double cashBpvTimeWAvg = affineDivUtils.CashBpvTimeWeightedAverage(maturity);
            double cashBpbAvg = affineDivUtils.CashBpvAverage(0.0, maturity);
            double volAdj = 1.0 + (cashBpvTimeWAvg - cashBpbAvg) / spot;

            double displacement = cashBpvTimeWAvg / volAdj;
            double formulaForward = growth * (spot - displacement);
            double formulaStrike = strike + growth * (cashBpv - displacement);
            double formulaVol = vol * volAdj;
            return BlackScholesOption.Price(formulaForward, formulaStrike, formulaVol, maturity, q);
        }

        #region private class
        private sealed class BsDivPrice
        {
            #region private fields
            private readonly double k;
            private readonly double a;
            private readonly double b;
            private readonly double midT;
            private readonly double dT;
            private readonly double maturity;
            private readonly double[] z;
            private readonly double[] quadWeights;
            #endregion

            public BsDivPrice(double maturity, double strike, double spot,
                AffineDivCurveUtils affineDivUtils, double[] quadPoints, double[] quadWeights)
            {
                this.quadWeights = quadWeights;
                this.maturity = maturity;

                midT = 0.5 * maturity; //TODO find a best heuristic !
                dT = maturity - midT;

                double sqrtMidT = Math.Sqrt(midT);
                z = quadPoints.Map(p => p * sqrtMidT);

                var displacement1 = affineDivUtils.CashBpvAverage(0.0, midT);
                var displacement2 = affineDivUtils.CashBpvAverage(midT, maturity);
                var maturityGrowth = affineDivUtils.AssetGrowth(maturity);

                k = strike + maturityGrowth * (affineDivUtils.CashDivBpv(maturity) - displacement2);
                a = maturityGrowth * (spot - displacement1);
                b = maturityGrowth * (displacement1 - displacement2);
            }
            private double Price(double volBeforeMidT, double volAfterMidT, double q)
            {
                double aCvx = a * Math.Exp(-0.5 * volBeforeMidT * volBeforeMidT * midT);

                var price = 0.0;
                for (int i = 0; i < z.Length; i++)
                {
                    double x = aCvx * Math.Exp(volBeforeMidT * z[i]) + b;
                    double midTprice = (x > 0.0) ? BlackScholesOption.Price(x, k, volAfterMidT, dT, q)
                        : (q > 0.0) ? 0.0 : k - x;
                    price += quadWeights[i] * midTprice;
                }
                return price;
            }
            public double Price(double vol, double q)
            {
                return Price(vol, vol, q);
            }
            public double Price(Func<double, double> terminalVol, double q)
            {
                var volBeforeMidT = terminalVol(midT);

                var termVolAtMaturity = terminalVol(maturity);
                var varAfterMidT = (termVolAtMaturity * termVolAtMaturity * maturity - volBeforeMidT * volBeforeMidT * midT) / dT;
                var volAfterMidT = Math.Sqrt(varAfterMidT);

                return Price(volBeforeMidT, volAfterMidT, q);
            }
        }
        #endregion
    }

    public class AffineDivCurveUtils
    {
        #region private fields
        private readonly Func<double, double> assetGrowth;
        private readonly StepFunction cashDivBpv;
        private readonly RrFunction cashBpvIntegral;
        //private readonly StepFunction squareTimeWeightedCash;
        #endregion
        public AffineDivCurveUtils(DividendQuote[] dividends,
                                   DiscountCurve discountCurve,
                                   ITimeMeasure time)
        {
            Contract.Requires(EnumerableUtils.IsSorted(dividends.Select(div => div.Date)));

            if (dividends.Length > 0)
            {
                double[] divDates = dividends.Map(div => time[div.Date]);
                double[] spotYieldGrowths = dividends.Scan(1.0, (prev, div) => prev * (1.0 - div.Yield));
                var spotYieldGrowth = new StepFunction(divDates, spotYieldGrowths, 1.0);
                assetGrowth = t => spotYieldGrowth.Eval(t) / discountCurve.Zc(t);

                double[] discountedCashs = dividends.Map(div => div.Cash / assetGrowth(time[div.Date]));
                double[] cashBpvs = discountedCashs.Scan(0.0, (prev, c) => prev + c);
                cashDivBpv = new StepFunction(divDates, cashBpvs, 0.0);
                cashBpvIntegral = cashDivBpv.Integral(0.0);

                //double[] squareTimeWeightedCashs = discountedCashs.ZipWith(divDates, (c, t) => c * t * t);
                //squareTimeWeightedCash = new StepFunction(divDates, squareTimeWeightedCashs, 0.0);
            }
            else
            {
                assetGrowth = t => 1.0 / discountCurve.Zc(t);
                cashDivBpv = new StepFunction(new[] { 0.0 }, new[] { 0.0 }, double.NaN);
                cashBpvIntegral = RrFunctions.Zero;
                //squareTimeWeightedCash = new StepFunction(new[] { 0.0 }, new[] { 0.0 }, double.NaN);
            }
        }

        public double AssetGrowth(double t)
        {
            return assetGrowth(t);
        }
        public double CashDivBpv(double t)
        {
            return cashDivBpv.Eval(t);
        }
        public double CashBpvAverage(double start, double end)
        {
            if (end > start)
                return (cashBpvIntegral.Eval(end) - cashBpvIntegral.Eval(start)) / (end - start);
            return cashBpvIntegral.Eval(start);
        }
        public double CashBpvTimeWeightedAverage(double t)
        {
            throw new NotImplementedException("todo");
            //return cashDivBpv.Eval(t) - squareTimeWeightedCash.Eval(t) / (t * t );
        }

        public void LehmanProxy(double maturity, double spot,
                                out double effectiveForward, out double strikeShift)
        {
            var growth = AssetGrowth(maturity);
            var cashBpvAvg = CashBpvAverage(0.0, maturity);

            effectiveForward = (spot - cashBpvAvg) * growth;
            strikeShift = growth * (CashDivBpv(maturity) - cashBpvAvg);
        }
    }
}