using System;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths;
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

        public AssetForwardCurve Forward(DiscountCurve cashFinancingCurve)
        {
            var assetFinancingCurve = DiscountCurve.Product(repoCurve, cashFinancingCurve);
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

    public class AssetForwardCurve
    {
        #region protected fields
        private readonly double spot;
        private readonly ITimeMeasure time;
        private readonly DividendQuote[] dividends;
        private readonly DiscountCurve assetFinancingCurve;
        private readonly RRFunction cumulatedDividendFunc;
        #endregion
        public AssetForwardCurve(double spot, DividendQuote[] dividends, DiscountCurve assetFinancingCurve,
            ITimeMeasure time)
        {
            this.spot = spot;
            this.dividends = dividends.OrderBy(d => d.Date).ToArray();
            this.assetFinancingCurve = assetFinancingCurve;
            this.time = time;

            var cumulatedDividends = new double[dividends.Length];
            var yieldGrowth = 1.0;
            var cumulDiv = 0.0;
            for (int i = 0; i < dividends.Length; i++)
            {
                var div = dividends[i];
                yieldGrowth *= 1.0 - div.Yield;
                cumulDiv += div.Cash / yieldGrowth * assetFinancingCurve.Zc(div.Date);
                cumulatedDividends[i] = cumulDiv;
            }
            cumulatedDividendFunc = new StepFunction(time[dividends.Select(d => d.Date).ToArray()], cumulatedDividends, 0.0);
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
            double growth = 1.0 / assetFinancingCurve.Zc(date);
            for (int i = 0; i < dividends.Length; i++)
            {
                var div = dividends[i];
                if (date > div.Date) break;
                growth *= 1.0 - div.Yield;
            }
            return growth;
        }
        public double CumulatedDividends(DateTime d)
        {
            return cumulatedDividendFunc.Eval(time[d]);
        }
    }
}