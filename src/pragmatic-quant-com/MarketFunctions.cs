using System;
using System.Diagnostics;
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
                       Category = "PragmaticQuant_Market")]
        public static bool SetMarket(string mktId, object range)
        {
            try
            {
                MarketManager.Instance.SetMarket(mktId, (object[,])range);
                return true;
            }
            catch (Exception e)
            {
                Trace.TraceError(e.Message); 
                return false;
            }
        }

        [ExcelFunction(Description = "Get market reference date",
                       Category = "PragmaticQuant_Market")]
        public static object MarketRefDate(object mktObj)
        {
            return FunctionRunnerUtils.Run("MarketRefDate", () =>
            {
                var market = MarketManager.Instance.GetMarket(mktObj);
                return market.RefDate;
            });
        }

        [ExcelFunction(Description = "Discount zero coupon function", 
                       Category = "PragmaticQuant_Market")]
        public static object Discount(object mktObj, object[,] dates, string curveId)
        {
            return FunctionRunnerUtils.Run("Discount", () =>
            {
                var market = MarketManager.Instance.GetMarket(mktObj);
                var finCurveId = FinancingId.Parse(curveId);
                DiscountCurve curve = market.DiscountCurve(finCurveId);

                var result = dates.Map(o =>
                {
                    var dateOrDuration = DateAndDurationConverter.ConvertDateOrDuration(o);
                    var date = dateOrDuration.ToDate(curve.RefDate);
                    return curve.Zc(date);
                });

                return result;
            });
        }

        [ExcelFunction(Description = "Equity Asset forward function",
                       Category = "PragmaticQuant_Market")]
        public static object AssetForward(object mktObj, object[,] dates, string assetName)
        {
            return FunctionRunnerUtils.Run("AssetForward", () =>
            {
                var market = MarketManager.Instance.GetMarket(mktObj);
                var assetMarket = market.AssetMarketFromName(assetName);

                AssetForwardCurve assetForward = assetMarket.Forward();

                var result = dates.Map(o =>
                {
                    var dateOrDuration = DateAndDurationConverter.ConvertDateOrDuration(o);
                    var date = dateOrDuration.ToDate(assetForward.RefDate);
                    return assetForward.Fwd(date);
                });

                return result;
            });
        }

        [ExcelFunction(Description = "Equity Asset vol function",
                       Category = "PragmaticQuant_Market")]
        public static object VolSurface(object mktObj, object[] dates, double[] strikes, string assetName)
        {
            return FunctionRunnerUtils.Run("VolSurface", () =>
            {
                Market market = MarketManager.Instance.GetMarket(mktObj);
                AssetMarket assetMarket = market.AssetMarketFromName(assetName);
                VolatilitySurface volSurface = assetMarket.VolSurface();
                var maturities = ObjectConverters.ConvertDateArray(dates, assetMarket.RefDate);

                return ArrayUtils.CartesianProd(maturities, strikes, volSurface.Volatility);
            });
        }
    }
}