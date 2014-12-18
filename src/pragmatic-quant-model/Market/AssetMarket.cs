using System;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.Market
{
    public class AssetMarket
    {
        #region private fields
        private readonly DateTime refDate;
        private readonly AssetId asset;
        private readonly double spot;
        private readonly DiscountProvider repoCurve;
        private readonly DividendDatas dividendDatas;
        #endregion
        public AssetMarket(DateTime refDate, AssetId asset, double spot, DiscountProvider repoCurve,
            DividendDatas dividendDatas)
        {
            this.refDate = refDate;
            this.asset = asset;
            this.spot = spot;
            this.repoCurve = repoCurve;
            this.dividendDatas = dividendDatas;
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
        public DiscountProvider RepoCurve
        {
            get { return repoCurve; }
        }
        
        public AssetForwardProvider Forward(FinancingCurveId financingCurve)
        {
            throw new NotImplementedException();
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
        }
        public readonly DateTime[] Dates;
        public readonly double[] CashPart;
        public readonly double[] YieldPart;
    }

    public abstract class AssetForwardProvider
    {
        #region protected fields
        protected readonly ITimeMeasure time;
        #endregion
        protected AssetForwardProvider(ITimeMeasure time)
        {
            this.time = time;
        }
        public DateTime RefDate
        {
            get { return time.RefDate; }
        }
        public abstract double Fwd(DateTime d);
    }
}