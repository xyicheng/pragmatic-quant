using System;
using System.Collections.Generic;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Factories
{
    public class MarketFactory : IFactoryFromBag<Market>
    {
        #region private methods
        private static DateTime RefDate(object[,] bag)
        {
            return BagServices.ProcessScalarDateOrDuration(bag, "RefDate").Date;
        }
        private static ITimeMeasure RateTimeMeasure(DateTime refDate)
        {
            return TimeMeasure.Act365(refDate);
        }
        private static IDictionary<FinancingCurveId, DiscountCurve> RateCurveFromRawDatas(TimeMatrixDatas curveRawDatas, DateTime refDate)
        {
            var rateTimeInterpol = RateTimeMeasure(refDate);
            DateTime[] datePillars = curveRawDatas.RowLabels
                .Select(dOrDur => dOrDur.ToDate(refDate)).ToArray();

            var curves = new Dictionary<FinancingCurveId, DiscountCurve>();
            foreach (var curveLabel in curveRawDatas.ColLabels)
            {
                FinancingCurveId financingId;
                if (!FinancingCurveId.TryParse(curveLabel, out financingId))
                    throw new ArgumentException(String.Format("RateMarketFactory, invalid Discount Curve Id : {0}", curveLabel));

                double[] zcs = curveRawDatas.GetCol(curveLabel);
                var discountCurve = DiscountCurve.LinearRateInterpol(datePillars, zcs, rateTimeInterpol);

                curves.Add(financingId, discountCurve);
            }
            return curves;
        }
        #endregion
        public Market Build(object[,] bag)
        {
            DateTime refDate = RefDate(bag);
            
            TimeMatrixDatas curveRawDatas = BagServices.ProcessTimeMatrixDatas(bag, "Discount");
            var discountCurves = RateCurveFromRawDatas(curveRawDatas, refDate);
            
            return new Market(discountCurves, null);
        }
    }
}
