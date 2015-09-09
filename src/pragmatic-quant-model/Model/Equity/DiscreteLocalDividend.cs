using System;
using System.Diagnostics.Contracts;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Function;

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
        private readonly Func<double, double> cumDiscCashAverage;
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

                var integral = cumDiscountedCash.Integral(0.0);
                cumDiscCashAverage = t => (t > 0.0) ? integral.Eval(t) / t : cumDiscountedCash.Eval(0.0);
            }
            else
            {
                assetGrowth = t => 1.0 / discountCurve.Zc(t);
                cumDiscountedCash = new StepFunction(new [] {0.0}, new [] {0.0}, double.NaN);
                cumDiscCashAverage = t => 0.0;
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
        public double CumDiscCashAverage(double t)
        {
            return cumDiscCashAverage(t);
        }
    }

}