using System;
using System.Collections.Generic;
using System.Linq;

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

            var curveDates = discountCurves.Values.Select(c => c.RefDate).ToArray();
            if (curveDates.Count() != 1)
                throw new Exception("RateMarket : many curve refDate's !");
            RefDate = curveDates.First();
        }
        public DiscountCurve DiscountCurve(FinancingCurveId financingId)
        {
            DiscountCurve curve;
            if (!discountCurves.TryGetValue(financingId, out curve))
                throw new Exception(string.Format("Missing discount curve : {0}", financingId));
            return curve;
        }
        public DateTime RefDate { get; private set; }
    }
}
