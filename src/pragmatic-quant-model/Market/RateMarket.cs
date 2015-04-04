using System;
using System.Collections.Generic;

namespace pragmatic_quant_model.Market
{
    public class RateMarket
    {
        #region private fields
        private readonly IDictionary<FinancingCurveId, DiscountCurve> discountCurves;
        #endregion
        public RateMarket(IDictionary<FinancingCurveId, DiscountCurve> discountCurves)
        {
            this.discountCurves = discountCurves;
        }
        public DiscountCurve DiscountCurve(FinancingCurveId financingId)
        {
            DiscountCurve curve;
            if (!discountCurves.TryGetValue(financingId, out curve))
                throw new Exception(string.Format("Missing discount curve : {0}", financingId));
            return curve;
        }
    }
}
