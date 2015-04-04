using System;
using System.Collections.Generic;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.Market;

namespace pragmatic_quant_model.Factories
{
    public class RateMarketFactory : IFactoryFromBag<RateMarket>
    {
        #region private methods
        private static DateTime RefDate(object[,] bag)
        {
            return BagServices.ProcessScalarDateOrDuration(bag, "RefDate").Date;
        }
        private static IDictionary<FinancingCurveId, DiscountCurve> BuildDiscount(object[,] bag, DateTime refDate)
        {
            var rateTimeInterpol = TimeMeasure.Act365(refDate);

            TimeMatrixDatas curveRawDatas = BagServices.ProcessTimeMatrixDatas(bag, "Discount");
            DateTime[] datePillars = curveRawDatas.RowLabels
                .Select(dOrDur => dOrDur.ToDate(refDate)).ToArray();

            var curves = new Dictionary<FinancingCurveId, DiscountCurve>();
            foreach (var curveLabel in curveRawDatas.ColLabels)
            {
                FinancingCurveId financingId;
                if (!FinancingCurveId.TryParse(curveLabel, out financingId))
                    throw new Exception(String.Format("RateMarketFactory, unable to parse Discount Curve Id : {0}", curveLabel));

                double[] zcs = curveRawDatas.GetCol(curveLabel);
                var discountCurve = DiscountCurve.LinearRateInterpol(datePillars, zcs, rateTimeInterpol);

                curves.Add(financingId, discountCurve);
            }
            return curves;
        }
        #endregion
        public RateMarket Build(object[,] bag)
        {
            DateTime refDate = RefDate(bag);
            IDictionary<FinancingCurveId, DiscountCurve> discountCurves = BuildDiscount(bag, refDate);
            return new RateMarket(discountCurves);
        }
    }
}
