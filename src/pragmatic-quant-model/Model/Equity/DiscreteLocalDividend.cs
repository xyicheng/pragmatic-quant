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
        private readonly StepFunction cumDiscountedCash;
        private readonly RrFunction cumDiscCashIntegral;
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
                double[] cumDiscountedCashs = discountedCashs.Scan(0.0, (prev, c) => prev + c);
                cumDiscountedCash = new StepFunction(divDates, cumDiscountedCashs, 0.0);
                cumDiscCashIntegral = cumDiscountedCash.Integral(0.0);
            }
            else
            {
                assetGrowth = t => 1.0 / discountCurve.Zc(t);
                cumDiscountedCash = new StepFunction(new [] {0.0}, new [] {0.0}, double.NaN);
                cumDiscCashIntegral = RrFunctions.Zero;
            }
        }

        public double AssetGrowth(double t)
        {
            return assetGrowth(t);
        }
        public double CumDiscountedCash(double t)
        {
            return cumDiscountedCash.Eval(t);
        }
        public double CumDiscCashAverage(double start, double end)
        {
            return (cumDiscCashIntegral.Eval(end) - cumDiscCashIntegral.Eval(start)) / (end - start);
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

            var displacement1 = affineDivUtils.CumDiscCashAverage(0.0, midT);
            var displacement2 = affineDivUtils.CumDiscCashAverage(midT, maturity);
            var maturityGrowth = affineDivUtils.AssetGrowth(maturity);

            var k = strike + maturityGrowth * (affineDivUtils.CumDiscountedCash(maturity) - displacement2);
            double a = maturityGrowth * (spot - displacement1) * Math.Exp(-0.5 * vol * vol * midT);
            double b = maturityGrowth * (displacement1 - displacement2);
            double stdDev = Math.Sqrt(midT) * vol;
            
            var price = 0.0;
            for (int i = 0; i < quadPoints.Length; i++)
            {
                double x = a * Math.Exp(stdDev * quadPoints[i]) + b;
                price += quadWeights[i] * BlackScholesOption.Price(x, k, vol, dT, q);
            }
            return price;
        }

        public double PriceLehman(double maturity, double strike, double vol, double q)
        {
            var growth = affineDivUtils.AssetGrowth(maturity);
            var cumDivAvg = affineDivUtils.CumDiscCashAverage(0.0, maturity);

            var formulaForward = (spot - cumDivAvg) * growth;
            var formulaStrike = strike + growth * (affineDivUtils.CumDiscountedCash(maturity) - cumDivAvg);
            return BlackScholesOption.Price(formulaForward, formulaStrike, vol, maturity, q);
        }
    }

}