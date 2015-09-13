using System;
using System.Diagnostics.Contracts;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Function;
using pragmatic_quant_model.Maths.Integration;

namespace pragmatic_quant_model.Model.Equity
{
    public abstract class DiscreteLocalDividend
    {
        #region protected method
        protected DiscreteLocalDividend(DateTime date)
        {
            Date = date;
        }
        #endregion

        public static DiscreteLocalDividend AffineDividend(DateTime date, double cash, double yield)
        {
            return new AffineLocalDividend(date, cash, yield);
        }
        public static DiscreteLocalDividend ZeroDiv(DateTime date)
        {
            return AffineDividend(date, 0.0, 0.0);
        }
        public abstract double Value(double spotValue);
        public DateTime Date { get; private set; }
        
        #region private class
        private class AffineLocalDividend : DiscreteLocalDividend
        {
            private readonly double cash;
            private readonly double yield;
            public AffineLocalDividend(DateTime date, double cash, double yield)
                : base(date)
            {
                this.cash = cash;
                this.yield = yield;
            }
            public override double Value(double spotValue)
            {
                return yield * spotValue + cash;
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
                cashDivBpv = new StepFunction(new[] {0.0}, new[] {0.0}, double.NaN);
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
            return (cashBpvIntegral.Eval(end) - cashBpvIntegral.Eval(start)) / (end - start);
        }
        public double CashBpvTimeWeightedAverage(double t)
        {
            throw new NotImplementedException("todo");
            //return cashDivBpv.Eval(t) - squareTimeWeightedCash.Eval(t) / (t * t );
        }
    }

    /// <summary>
    /// Pricer for vanilla with constant Black-Scholes model with dividends.
    /// </summary>
    public class BlackScholesWithDividendOption
    {
        #region private fields
        private readonly double spot;
        private readonly AffineDivCurveUtils affineDivUtils;
        private readonly double[] quadPoints;
        private readonly double[] quadWeights;
        #endregion
        public BlackScholesWithDividendOption(double spot, AffineDivCurveUtils affineDivUtils, int quadratureNbPoints)
        {
            this.affineDivUtils = affineDivUtils;
            this.spot = spot;
            GaussHermite.GetQuadrature(quadratureNbPoints, out quadPoints, out quadWeights);
        }
        
        /// <param name="maturity">maturity</param>
        /// <param name="strike">strike</param>
        /// <param name="vol">volatility</param>
        /// <param name="q"> 1 for call, -1 for put </param>
        /// <returns></returns>
        public double Price(double maturity, double strike, double vol, double q)
        {
            var midT = 0.5 * maturity; //TODO find a best heuristic !
            var dT = maturity - midT;

            var displacement1 = affineDivUtils.CashBpvAverage(0.0, midT);
            var displacement2 = affineDivUtils.CashBpvAverage(midT, maturity);
            var maturityGrowth = affineDivUtils.AssetGrowth(maturity);

            var k = strike + maturityGrowth * (affineDivUtils.CashDivBpv(maturity) - displacement2);
            double a = maturityGrowth * (spot - displacement1) * Math.Exp(-0.5 * vol * vol * midT);
            double b = maturityGrowth * (displacement1 - displacement2);
            double stdDev = Math.Sqrt(midT) * vol;
            
            var price = 0.0;
            for (int i = 0; i < quadPoints.Length; i++)
            {
                double x = a * Math.Exp(stdDev * quadPoints[i]) + b;
                double midTprice = (x > 0.0) ? BlackScholesOption.Price(x, k, vol, dT, q)
                                             : (q > 0.0) ? 0.0 : k - x;
                price += quadWeights[i] * midTprice;
            }
            return price;
        }

        public double PriceLehman(double maturity, double strike, double vol, double q)
        {
            var growth = affineDivUtils.AssetGrowth(maturity);
            var cashBpvAvg = affineDivUtils.CashBpvAverage(0.0, maturity);

            var formulaForward = (spot - cashBpvAvg) * growth;
            var formulaStrike = strike + growth * (affineDivUtils.CashDivBpv(maturity) - cashBpvAvg);
            return BlackScholesOption.Price(formulaForward, formulaStrike, vol, maturity, q);
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
    }

}