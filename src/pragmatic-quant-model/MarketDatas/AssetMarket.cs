using System;
using System.Diagnostics.Contracts;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Maths.Function;

namespace pragmatic_quant_model.MarketDatas
{
    public class AssetMarket
    {
        #region private fields
        private readonly DateTime refDate;
        private readonly AssetId asset;
        private readonly ITimeMeasure time;
        private readonly double spot;
        private readonly DiscountCurve repoCurve;
        private readonly DividendQuote[] dividends;
        #endregion
        
        public AssetMarket(AssetId asset, DateTime refDate, ITimeMeasure time,
            double spot, DiscountCurve repoCurve, DividendQuote[] dividends)
        {
            this.asset = asset;
            this.refDate = refDate;
            this.time = time;
            this.spot = spot;
            this.repoCurve = repoCurve;
            this.dividends = dividends;
            if (refDate != repoCurve.RefDate || refDate != time.RefDate)
                throw new Exception("AssetMarket : incompatible ref date !");
        }

        public DateTime RefDate
        {
            get { return refDate; }
        }
        public AssetId Asset
        {
            get { return asset; }
        }
        public double Spot
        {
            get { return spot; }
        }
        public DividendQuote[] Dividends
        {
            get { return dividends; }
        }
        public DiscountCurve RepoCurve
        {
            get { return repoCurve; }
        }

        public DiscountCurve AssetFinancingCurve(DiscountCurve cashFinancingCurve)
        {
            return DiscountCurve.Product(repoCurve, cashFinancingCurve, FinancingId.AssetCollat(asset));
        }
        public AssetForwardCurve Forward(DiscountCurve cashFinancingCurve)
        {
            var assetFinancingCurve = AssetFinancingCurve(cashFinancingCurve);
            return new AssetForwardCurve(spot, dividends, assetFinancingCurve, time);
        }
    }

    public class DividendQuote
    {

        public DividendQuote(DateTime date, double cash, double yield)
        {
            Date = date;
            Yield = yield;
            Cash = cash;
        }
        
        public DateTime Date { get; private set; }
        public double Cash { get; private set; }
        public double Yield { get; private set; }
    }

    public class VolatilityQuote
    {
        
    }

    public class AssetForwardCurve
    {
        #region private fields
        private readonly double spot;
        private readonly ITimeMeasure time;
        private readonly DiscountCurve assetFinancingCurve;

        private readonly StepFunction yieldGrowthFunc;
        private readonly StepFunction cumulatedDividendFunc;
        #endregion
        
        public AssetForwardCurve(double spot, DividendQuote[] dividends, DiscountCurve assetFinancingCurve,
            ITimeMeasure time)
        {
            Contract.Requires(EnumerableUtils.IsSorted(dividends.Select(div => div.Date)));

            this.spot = spot;
            this.assetFinancingCurve = assetFinancingCurve;
            this.time = time;

            double[] yieldGrowths = dividends.Scan(1.0, (prev, div) => prev * (1.0 - div.Yield));
            double[] discountedDivCash = dividends.ZipWith(yieldGrowths,
                (div, yGrowth) => div.Cash / yGrowth * assetFinancingCurve.Zc(div.Date));
            double[] cumulatedDividends = discountedDivCash.Scan(0.0, (prev, c) => prev + c);
            var divDates = time[dividends.Select(d => d.Date).ToArray()];

            yieldGrowthFunc = new StepFunction(divDates, yieldGrowths, 1.0);
            cumulatedDividendFunc = new StepFunction(divDates, cumulatedDividends, 0.0);
        }

        public DateTime RefDate
        {
            get { return time.RefDate; }
        }
        public double Fwd(DateTime d)
        {
            return (spot - CumulatedDividends(d)) * AssetGrowth(d);
        }
        public double AssetGrowth(DateTime date)
        {
            return yieldGrowthFunc.Eval(time[date]) / assetFinancingCurve.Zc(date);
        }
        public double CumulatedDividends(DateTime d)
        {
            return cumulatedDividendFunc.Eval(time[d]);
        }

    }
}