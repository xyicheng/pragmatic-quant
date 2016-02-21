using System;
using ExcelDna.Integration;
using pragmatic_quant_com.Factories;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Model;
using pragmatic_quant_model.Model.Equity.Bergomi;
using pragmatic_quant_model.Model.Equity.Dividends;

namespace pragmatic_quant_com
{
    public class ModelFunctions
    {
        [ExcelFunction(Description = "Equity vanilla option",
            Category = "PragmaticQuant_Model")]
        public static object EquityVanillaOption(object mktObj, string assetName, object maturity, double strike, double vol, string optionType)
        {
            return FunctionRunnerUtils.Run("EquityVanillaOption", () =>
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

                var matAsDate = ObjectConverters.ConvertDate(maturity, assetMkt.RefDate);
                return pricer.Price(assetMkt.Time[matAsDate], strike, vol, q);
            });
        }

        [ExcelFunction(Description = "Equity vanilla option implied volatility",
            Category = "PragmaticQuant_Model")]
        public static object EquityImpliedVol(object mktObj, string assetName, object maturity, double strike, double price, string optionType)
        {
            return FunctionRunnerUtils.Run("EquityImpliedVol", () =>
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

                var matAsDate = ObjectConverters.ConvertDate(maturity, assetMkt.RefDate);
                return pricer.ImpliedVol(assetMkt.Time[matAsDate], strike, price, q);
            });
        }
        
        [ExcelFunction(Description = "Equity local volatility function",
            Category = "PragmaticQuant_Model")]
        public static object LocalVolSurface(object mktObj, object[] dates, double[] strikes, string assetName)
        {
            return FunctionRunnerUtils.Run("LocalVolSurface", () =>
            {
                Market market = MarketManager.Instance.GetMarket(mktObj);
                AssetMarket assetMarket = market.AssetMarketFromName(assetName);
                VolatilitySurface volSurface = assetMarket.VolSurface();
                var maturities = ObjectConverters.ConvertDateArray(dates, assetMarket.RefDate);

                return ArrayUtils.CartesianProd(maturities, strikes, volSurface.LocalVol);
            });
        }

        [ExcelFunction(Description = "Equity local volatility function",
            Category = "PragmaticQuant_Model")]
        public static object LocalVolDenominator(object mktObj, object[] dates, double[] strikes, string assetName)
        {
            return FunctionRunnerUtils.Run("LocalVolSurface", () =>
            {
                Market market = MarketManager.Instance.GetMarket(mktObj);
                AssetMarket assetMarket = market.AssetMarketFromName(assetName);
                VolatilitySurface volSurface = assetMarket.VolSurface();
                
                var localVariance = volSurface.LocalVariance;
                var moneyness = volSurface.Moneyness;
                var time = volSurface.Time;

                var result = new double[dates.Length, strikes.Length];
                for (int i = 0; i<dates.Length; i++)
                {
                    var date = ObjectConverters.ConvertDate(dates[i], assetMarket.RefDate);
                    double t = time[date];
                    var denomFunc = localVariance.Denominator(t);
                    var denoms = strikes.Map(k =>
                    {
                        var y = moneyness.Moneyness(t, k);
                        return denomFunc.Eval(y);
                    });
                    ArrayUtils.SetRow(ref result, i, denoms);
                }
                return result;
            });
        }
    }

    public static class BergomiModelFunctions
    {
        [ExcelFunction(Description = "Bergomi 2F model Vol Of Vol Viewer",
            Category = "PragmaticQuant_Model")]
        public static object Bergomi2FVolOfVol(object mktObj, object[,] modelBag, object[] starts, object[] ends)
        {
            return FunctionRunnerUtils.Run("Bergomi2FVolOfVol", () =>
            {
                Market market = MarketManager.Instance.GetMarket(mktObj);
                ICalibrationDescription b2FCalibDesc = Bergomi2FModelFactoryFromBag.Instance.Build(modelBag);
                IModelDescription b2FDesc = ModelCalibration.Instance.Calibrate(b2FCalibDesc, market);
                var b2Fmodel = (Bergomi2FModel) ModelFactory.Instance.Build(b2FDesc, market);

                var startDates = b2Fmodel.Time[ObjectConverters.ConvertDateArray(starts, market.RefDate)];
                var endDates = b2Fmodel.Time[ObjectConverters.ConvertDateArray(ends, market.RefDate)];
                if (startDates.Length != endDates.Length)
                    throw new ArgumentException("Incompatible size between starts and ends");

                return Bergomi2FUtils.FwdVolInstantVol(b2Fmodel, startDates, endDates).AsColumn();
            });
        }

        [ExcelFunction(Description = "Bergomi 2F model Correlation Of Vol Viewer",
            Category = "PragmaticQuant_Model")]
        public static object Bergomi2FCorrelOfVol(object mktObj, object[,] modelBag, object[] starts, object[] ends)
        {
            return FunctionRunnerUtils.Run("Bergomi2FCorrelOfVol", () =>
            {
                Market market = MarketManager.Instance.GetMarket(mktObj);
                ICalibrationDescription b2FCalibDesc = Bergomi2FModelFactoryFromBag.Instance.Build(modelBag);
                IModelDescription b2FDesc = ModelCalibration.Instance.Calibrate(b2FCalibDesc, market);
                var b2Fmodel = (Bergomi2FModel)ModelFactory.Instance.Build(b2FDesc, market);
                
                var startDates = b2Fmodel.Time[ObjectConverters.ConvertDateArray(starts, market.RefDate)];
                var endDates = b2Fmodel.Time[ObjectConverters.ConvertDateArray(ends, market.RefDate)];
                if (startDates.Length != endDates.Length)
                    throw new ArgumentException("Incompatible size between starts and ends");

                var covar = Bergomi2FUtils.FwdVolInstantCovariance(b2Fmodel, startDates, endDates);
                var correl = covar.Correlation();
                return correl;
            });
        }

        [ExcelFunction(Description = "Bergomi 2F model Correlation Of Vol Viewer",
            Category = "PragmaticQuant_Model")]
        public static object Bergomi2FAtmfSkewApprox(object mktObj, object[,] modelBag, object[] maturities)
        {
            return FunctionRunnerUtils.Run("Bergomi2FAtmfSkewApprox", () =>
            {
                Market market = MarketManager.Instance.GetMarket(mktObj);
                ICalibrationDescription b2FCalibDesc = Bergomi2FModelFactoryFromBag.Instance.Build(modelBag);
                IModelDescription b2FDesc = ModelCalibration.Instance.Calibrate(b2FCalibDesc, market);
                var b2Fmodel = (Bergomi2FModel)ModelFactory.Instance.Build(b2FDesc, market);

                var mats = b2Fmodel.Time[ObjectConverters.ConvertDateArray(maturities, market.RefDate)];
                
                var skews = Bergomi2FUtils.AtmfSkewApprox(b2Fmodel, mats);
                return skews.AsColumn();
            });
        }
    }
}