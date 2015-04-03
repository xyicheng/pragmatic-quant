using System;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Function;

namespace pragmatic_quant_model.Market
{
    public class AssetMarket
    {
        #region private fields
        private readonly DateTime refDate;
        private readonly AssetId asset;
        private readonly ITimeMeasure time;
        private readonly double spot;
        private readonly DiscountCurve repoCurve;
        private readonly DividendDatas dividendDatas;
        #endregion
        public AssetMarket(AssetId asset, DateTime refDate, ITimeMeasure time,
            double spot, DiscountCurve repoCurve, DividendDatas dividendDatas)
        {
            this.asset = asset;
            this.refDate = refDate;
            this.time = time;
            this.spot = spot;
            this.repoCurve = repoCurve;
            this.dividendDatas = dividendDatas;
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
        public DividendDatas DividendDatas
        {
            get { return dividendDatas; }
        }
        public DiscountCurve RepoCurve
        {
            get { return repoCurve; }
        }
        public AssetForwardProvider Forward(DiscountCurve cashFinancingCurve)
        {
            var assetFinancingCurve = DiscountCurve.Product(repoCurve, cashFinancingCurve);
            return new AssetForwardProvider(spot, dividendDatas, assetFinancingCurve, time);
        }
    }

    public class DividendDatas
    {
        public DividendDatas(DateTime[] dates, double[] cashPart, double[] yieldPart)
        {
            Dates = dates;
            CashPart = cashPart;
            YieldPart = yieldPart;
            if (dates.Length != cashPart.Length || cashPart.Length != yieldPart.Length)
                throw new Exception("DividendDatas : incompatible data size !");
            if (!EnumerableUtils.IsSorted(dates))
                throw new Exception("DividendDatas : dividend dates must be sorted !");
        }
        public readonly DateTime[] Dates;
        public readonly double[] CashPart;
        public readonly double[] YieldPart;
    }

    public class AssetForwardProvider
    {
        #region protected fields
        private readonly double spot;
        private readonly ITimeMeasure time;
        private readonly DividendDatas dividendDatas;
        private readonly DiscountCurve assetFinancingCurve;
        private readonly RRFunction cumulatedDividendFunc;
        #endregion
        public AssetForwardProvider(double spot, DividendDatas dividendDatas, DiscountCurve assetFinancingCurve,
            ITimeMeasure time)
        {
            this.spot = spot;
            this.dividendDatas = dividendDatas;
            this.assetFinancingCurve = assetFinancingCurve;
            this.time = time;

            var cumulatedDividends = new double[dividendDatas.Dates.Length];
            var yieldGrowth = 1.0;
            var cumulDiv = 0.0;
            for (int i = 0; i < dividendDatas.Dates.Length; i++)
            {
                yieldGrowth *= 1.0 - dividendDatas.YieldPart[i];
                cumulDiv += dividendDatas.CashPart[i] / yieldGrowth * assetFinancingCurve.Zc(dividendDatas.Dates[i]);
                cumulatedDividends[i] = cumulDiv;
            }
            cumulatedDividendFunc = new StepFunction(time[dividendDatas.Dates], cumulatedDividends, 0.0);
        }
        public DateTime RefDate
        {
            get { return time.RefDate; }
        }
        public double Fwd(DateTime d)
        {
            return (spot - CumulatedDividends(d)) * AssetGrowth(d);
        }
        public double AssetGrowth(DateTime d)
        {
            double growth = 1.0 / assetFinancingCurve.Zc(d);
            for (int i = 0; i < dividendDatas.YieldPart.Length; i++)
            {
                if (d > dividendDatas.Dates[i]) break;
                growth *= 1.0 - dividendDatas.YieldPart[i];
            }
            return growth;
        }
        public double CumulatedDividends(DateTime d)
        {
            return cumulatedDividendFunc.Eval(time[d]);
        }
    }
}