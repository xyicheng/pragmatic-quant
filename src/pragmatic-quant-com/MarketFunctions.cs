using System;
using System.Collections.Generic;
using ExcelDna.Integration;
using pragmatic_quant_com.Factories;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_com
{
    public class MarketFunctions
    {
        [ExcelFunction(Description = "Set market from xl range", 
                       Category = "PragmaticQuant_MarketFunctions")]
        public static bool SetMarket(string mktId, object range)
        {
            try
            {
                MarketManager.Value.SetMarket(mktId, (object[,])range);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        [ExcelFunction(Description = "Discount zero coupon function", 
                       Category = "PragmaticQuant_MarketFunctions")]
        public static object Discount(object mktObj, object[,] dates, string curveId)
        {
            try
            {
                var market = MarketManager.Value.GetMarket(mktObj);
                var finCurveId = FinancingId.Parse(curveId);
                DiscountCurve curve = market.DiscountCurve(finCurveId);

                var result = dates.Map(o =>
                {
                    var dateOrDuration = DateAndDurationConverter.ConvertDateOrDuration(o);
                    var date = dateOrDuration.ToDate(curve.RefDate);
                    return curve.Zc(date);
                });

                return result;
            }
            catch (Exception e)
            {
                var error = new object[1, 1];
                error[0, 0] = string.Format("ERROR, {0}", e.Message);
                return error;
            }
        }

        [ExcelFunction(Description = "Equity Asset forward function",
                       Category = "PragmaticQuant_MarketFunctions")]
        public static object AssetForward(object mktObj, object[,] dates, string assetName)
        {
            try
            {
                var market = MarketManager.Value.GetMarket(mktObj);
                var assetMarket = market.AssetMarketFromName(assetName);

                Currency assetCurrency = assetMarket.Asset.Currency;
                DiscountCurve cashCurve = market.DiscountCurve(FinancingId.RiskFree(assetCurrency));
                AssetForwardCurve assetForward = assetMarket.Forward(cashCurve);

                var result = dates.Map(o =>
                {
                    var dateOrDuration = DateAndDurationConverter.ConvertDateOrDuration(o);
                    var date = dateOrDuration.ToDate(assetForward.RefDate);
                    return assetForward.Fwd(date);
                });

                return result;
            }
            catch (Exception e)
            {
                var error = new object[1, 1];
                error[0, 0] = string.Format("ERROR, {0}", e.Message);
                return error;
            }
        }
    }

    public sealed class MarketManager
    {
        #region private fields
        private readonly IDictionary<string, Market> marketByIds;
        #endregion
        #region private methods
        private string FormattedId(string id)
        {
            return id.Trim().ToLowerInvariant();
        }
        private MarketManager()
        {
            marketByIds= new Dictionary<string, Market>();
        }
        #endregion

        public static MarketManager Value = new MarketManager();

        public void SetMarket(string mktId, object[,] marketBag)
        {
            var market = MarketFactory.Value.Build(marketBag);

            var formatedId = FormattedId(mktId);
            if (marketByIds.ContainsKey(formatedId))
            {
                marketByIds[formatedId] = market;
            }
            else
            {
                lock (this)
                {
                    marketByIds.Add(formatedId, market);
                }
            }
        }
        public bool HasMarket(string mktId)
        {
            return marketByIds.ContainsKey(FormattedId(mktId));
        }
        public Market GetMarket(object mktObj)
        {
            var mktBag = mktObj as object[,];
            if (mktBag != null)
                return MarketFactory.Value.Build(mktBag);

            if (!(mktObj is string))
                throw new ApplicationException(string.Format("Unable to build market from : {0}", mktObj));

            var mktId = mktObj as String;

            Market mkt;
            if (!marketByIds.TryGetValue(FormattedId(mktId), out mkt))
            {
                throw new ApplicationException(string.Format("No market for id : {0}", mktId));
            }
            return mkt;
        }
    }
}