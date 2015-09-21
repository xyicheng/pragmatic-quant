using System;
using ExcelDna.Integration;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Model.Equity;

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
                MarketManager.Instance.SetMarket(mktId, (object[,])range);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        [ExcelFunction(Description = "Get market reference date",
                       Category = "PragmaticQuant_MarketFunctions")]
        public static object MarketRefDate(object mktObj)
        {
            try
            {
                var market = MarketManager.Instance.GetMarket(mktObj);
                return market.RefDate;
            }
            catch (Exception e)
            {
                var error = new object[1, 1];
                error[0, 0] = string.Format("ERROR, {0}", e.Message);
                return error;
            }
        }

        [ExcelFunction(Description = "Discount zero coupon function", 
                       Category = "PragmaticQuant_MarketFunctions")]
        public static object Discount(object mktObj, object[,] dates, string curveId)
        {
            try
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
            }
            catch (Exception e)
            {
                var error = new object[1, 1];
                error[0, 0] = string.Format("ERROR, {0}", e.Message);
                return error;
            }
        }

        [ExcelFunction(Description = "Equity Asset vol function",
                       Category = "PragmaticQuant_MarketFunctions")]
        public static object VolSurface(object mktObj, object[] dates, double[] strikes, string assetName)
        {
            try
            {
                Market market = MarketManager.Instance.GetMarket(mktObj);
                AssetMarket assetMarket = market.AssetMarketFromName(assetName);
                VolatilitySurface volSurface = assetMarket.VolSurface();
                
                var maturities = dates.Map(o =>
                {
                    var dateOrDuration = DateAndDurationConverter.ConvertDateOrDuration(o);
                    return dateOrDuration.ToDate(assetMarket.RefDate);
                });
                
                return ArrayUtils.CartesianProd(maturities, strikes, volSurface.Volatility);
            }
            catch (Exception e)
            {
                var error = new object[1, 1];
                error[0, 0] = string.Format("ERROR, {0}", e.Message);
                return error;
            }
        }
    }

    public class ModelFunctions
    {
        [ExcelFunction(Description = "Equity local volatility function",
                       Category = "PragmaticQuant_ModelFunctions")]
        public static object LocalVolSurface(object mktObj, object[] dates, double[] strikes, string assetName)
        {
            try
            {
                Market market = MarketManager.Instance.GetMarket(mktObj);
                AssetMarket assetMarket = market.AssetMarketFromName(assetName);
                VolatilitySurface volSurface = assetMarket.VolSurface();

                var maturities = dates.Map(o =>
                {
                    var dateOrDuration = DateAndDurationConverter.ConvertDateOrDuration(o);
                    return dateOrDuration.ToDate(assetMarket.RefDate);
                });

                return ArrayUtils.CartesianProd(maturities, strikes, volSurface.LocalVol);
            }
            catch (Exception e)
            {
                var error = new object[1, 1];
                error[0, 0] = string.Format("ERROR, {0}", e.Message);
                return error;
            }
        }

        [ExcelFunction(Description = "Equity vanilla option",
                       Category = "PragmaticQuant_ModelFunctions")]
        public static object EquityVanillaOption(object mktObj, string assetName, object maturity, double strike, double vol, string optionType)
        {
            try
            {
                Market market = MarketManager.Instance.GetMarket(mktObj);
                AssetMarket assetMkt = market.AssetMarketFromName(assetName);
                var pricer = BlackScholesWithDividendOption.Build(assetMkt.Spot,
                                                                  assetMkt.Dividends,
                                                                  assetMkt.RiskFreeDiscount,
                                                                  assetMkt.Time);
                double q;
                switch (optionType.Trim().ToLower())
                {
                    case "call":
                        q = 1.0;
                        break;
                    case "put":
                        q = -1.0;
                        break;
                    default:
                        throw new Exception(string.Format("Unknow option type : {0}", optionType));
                }

                var matAsDate = DateAndDurationConverter.ConvertDateOrDuration(maturity)
                                                  .ToDate(assetMkt.RefDate);
                return pricer.Price(assetMkt.Time[matAsDate], strike, vol, q);
            }
            catch (Exception e)
            {
                return string.Format("FAILURE '{0}'", e.Message);
            }
        }
    }
}