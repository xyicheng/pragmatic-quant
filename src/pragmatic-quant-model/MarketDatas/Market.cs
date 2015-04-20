using System;
using System.Collections.Generic;
using System.Linq;

namespace pragmatic_quant_model.MarketDatas
{
    public class Market
    {
        #region private fields
        private readonly IDictionary<FinancingCurveId, DiscountCurve> discountCurves;
        private readonly IDictionary<AssetId, AssetMarket> assetMkts;
        #endregion
        public Market(IDictionary<FinancingCurveId, DiscountCurve> discountCurves,
                      IDictionary<AssetId, AssetMarket> assetMkts)
        {
            this.discountCurves = discountCurves;
            this.assetMkts = assetMkts;

            RefDate = discountCurves.First().Value.RefDate;

            if (discountCurves.Values.Any(c => c.RefDate != RefDate))
                throw new Exception("RateMarket : many curve refDate's !");

            if (assetMkts.Values.Any(m => m.RefDate != RefDate))
                throw new Exception("AssetMarket : many curve refDate's !");
        }

        public DiscountCurve DiscountCurve(FinancingCurveId financingId)
        {
            DiscountCurve curve;
            if (!discountCurves.TryGetValue(financingId, out curve))
                throw new Exception(string.Format("Missing discount curve : {0}", financingId));
            return curve;
        }
        public AssetMarket AssetMarket(AssetId asset)
        {
            AssetMarket assetMkt;
            if (!assetMkts.TryGetValue(asset, out assetMkt))
                throw new Exception(string.Format("Missing market asset : {0}", asset));
            return assetMkt;
        }
        
        public DateTime RefDate { get; private set; }
        public FinancingCurveId[] DiscountCurveIds
        {
            get { return discountCurves.Keys.ToArray(); }
        }
        public AssetId[] AssetIds
        {
            get { return assetMkts.Keys.ToArray(); }
        }
    }

    public static class MarketUtils
    {
        public static AssetMarket AssetMarketFromName(this Market mkt, string assetName)
        {
            var ids = mkt.AssetIds.Where(id => id.Name.Equals(assetName)).ToArray();
            if (ids.Length == 0)
                throw new Exception(string.Format("Missing market asset : {0}", assetName));
            if (ids.Length >1)
                throw new Exception(string.Format("Ambiguous asset name : {0}", assetName));

            return mkt.AssetMarket(ids.First());
        }
    }

}
