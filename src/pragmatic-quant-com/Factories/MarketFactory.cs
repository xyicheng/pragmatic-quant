using System;
using System.Linq;
using System.Collections.Generic;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_com.Factories
{
    using TimeMatrixDatas = LabelledMatrix<DateOrDuration, string, double>;

    public class MarketFactory : Singleton<MarketFactory>, IFactoryFromBag<Market>
    {
        #region private methods
        private static DateTime RefDate(object[,] bag)
        {
            return bag.ProcessScalarDateOrDuration("RefDate").Date;
        }
        private static ITimeMeasure RateTimeMeasure(DateTime refDate)
        {
            return TimeMeasure.Act365(refDate);
        }
        private static DiscountCurve[] RateCurveFromRawDatas(TimeMatrixDatas curveRawDatas, DateTime refDate)
        {
            var rateTimeInterpol = RateTimeMeasure(refDate);
            DateTime[] datePillars = curveRawDatas.RowLabels
                .Select(dOrDur => dOrDur.ToDate(refDate)).ToArray();

            var curves = new List<DiscountCurve>();
            foreach (var curveLabel in curveRawDatas.ColLabels)
            {
                FinancingId financingId;
                if (!FinancingId.TryParse(curveLabel, out financingId))
                    throw new ArgumentException(String.Format("RateMarketFactory, invalid Discount Curve Id : {0}", curveLabel));

                double[] zcs = curveRawDatas.GetColFromLabel(curveLabel);
                var discountCurve = DiscountCurve.LinearRateInterpol(financingId, datePillars, zcs, rateTimeInterpol);

                curves.Add(discountCurve);
            }
            return curves.ToArray();
        }
        private static DividendQuote[] ProcessDiv(object[,] bag, DateTime refDate, string assetName)
        {
            var divId = String.Format("Dividend.{0}", assetName);
            var dividendsRawDatas = bag.ProcessTimeMatrixDatas(divId);
            var cashs = dividendsRawDatas.GetColFromLabel("Cash");
            var yields = dividendsRawDatas.GetColFromLabel("Yield");
            var divQuotes = dividendsRawDatas.RowLabels.Select((d, idx) =>
                new DividendQuote(d.ToDate(refDate), cashs[idx], yields[idx])).ToArray();
            return divQuotes;
        }
        private static VolatilityMatrix AssetVolMatrix(object[,] bag, ITimeMeasure time, string assetName)
        {
            var volId = String.Format("Vol.{0}", assetName);
            var eqtyVolMatrix = bag.ProcessEqtyVolMatrix(volId);
            var pillars = eqtyVolMatrix.RowLabels.Map(d => d.ToDate(time.RefDate));
            return new VolatilityMatrix(time, pillars, eqtyVolMatrix.ColLabels, eqtyVolMatrix.Values);
        }
        private static AssetMarket[] ProcessAssetMkt(object[,] bag, DateTime refDate, DiscountCurve[] discountCurves)
        {
            var eqtyTime = TimeMeasure.Act365(refDate);
            var assetRawDatas = bag.ProcessLabelledMatrix("Asset", o => o.ToString(), o => o.ToString(), o => o);

            TimeMatrixDatas repoRawDatas = bag.ProcessTimeMatrixDatas("Repo");
            var repoPillars = repoRawDatas.RowLabels
                .Select(dOrDur => dOrDur.ToDate(refDate)).ToArray();

            var assetMkts = new List<AssetMarket>();
            for (int i = 0; i < assetRawDatas.RowLabels.Length; i++)
            {
                var assetName = assetRawDatas.RowLabels[i];
                var rawCurrency = assetRawDatas.GetColFromLabel("Currency")[i].ToString();
                object rawSpot = assetRawDatas.GetColFromLabel("Spot")[i];

                var currency = Currency.Parse(rawCurrency);
                var assetId = new AssetId(assetName, currency);

                double spot;
                if (!NumberConverter.TryConvertDouble(rawSpot, out spot))
                    throw new ArgumentException(String.Format("AssetMarketFactory, invalid {0} spot : {1}", assetName, rawSpot));

                double[] repoRates = repoRawDatas.GetColFromLabel(assetName);
                var repoZcs = repoRates.Select((r, idx) => Math.Exp(-eqtyTime[repoPillars[idx]] * r)).ToArray();
                var repoCurve = DiscountCurve.LinearRateInterpol(FinancingId.AssetCollat(assetId), repoPillars, repoZcs, eqtyTime);

                var divQuotes = ProcessDiv(bag, refDate, assetName);
                var volMatrix = AssetVolMatrix(bag, eqtyTime, assetName);

                var riskFreeDiscount = discountCurves.Single(curve => curve.Financing.Equals(FinancingId.RiskFree(currency)));

                var mkt = new AssetMarket(assetId, refDate, eqtyTime,
                                          spot, riskFreeDiscount, repoCurve, 
                                          divQuotes, volMatrix);
                assetMkts.Add(mkt);
            }

            return assetMkts.ToArray();
        }
        #endregion
        public Market Build(object[,] bag)
        {
            DateTime refDate = RefDate(bag);

            TimeMatrixDatas curveRawDatas = bag.ProcessTimeMatrixDatas("Discount");
            DiscountCurve[] discountCurves = RateCurveFromRawDatas(curveRawDatas, refDate);

            AssetMarket[] assetMarket = ProcessAssetMkt(bag, refDate, discountCurves);
            
            return new Market(discountCurves, assetMarket);
        }
    }

}
