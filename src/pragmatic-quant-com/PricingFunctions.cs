using System;
using System.Diagnostics;
using System.Threading.Tasks;
using ExcelDna.Integration;
using pragmatic_quant_com.Factories;
using pragmatic_quant_com.Publishers;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Model;
using pragmatic_quant_model.Pricing;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_com
{
    public class PricingFunctions
    {
        #region private methods
        private static Task<Market> BuildMarket(TaskFactory taskFactory, object mktObj)
        {
            return taskFactory.ComputationTaskWithLog("market preparation",
                () => MarketManager.Instance.GetMarket(mktObj));
        }
        private static Task<ICalibrationDescription> BuildCalibration(TaskFactory taskFactory, object[,] modelBag)
        {
            return taskFactory.ComputationTaskWithLog("model preparation",
                () => ModelCalibrationFactory.Instance.Build(modelBag));
        }
        private static Task<IPricer> BuildPricer(TaskFactory taskFactory, object requestObj, object[,] algorithmBag)
        {
            return taskFactory.ComputationTaskWithLog("pricer preparation", () =>
            {
                INumericalMethodConfig algorithm = AlgorithmFactory.Instance.Build(algorithmBag);

                var mcConfig = algorithm as MonteCarloConfig;
                if (mcConfig != null)
                {
                    if (!requestObj.ToString().Equals("price", StringComparison.InvariantCultureIgnoreCase))
                        throw new Exception("Unknow request");

                    return (IPricer) McPricer.WithDetails(mcConfig);
                }

                throw new Exception(string.Format("Unknown numerical method !"));
            });
        }
        private static Task<IProduct> BuildProduct(TaskFactory taskFactory, object[,] productBag)
        {
            return taskFactory.ComputationTaskWithLog("Product preparation",
                () => ProductFactory.Instance.Build(productBag));
        }
        private static Task<IModel> CalibrateModel(TaskFactory taskFactory, Task<Market> market, Task<ICalibrationDescription> modelCalibDesc)
        {
            return taskFactory.ComputationTaskWithLog("Model calibration", () =>
            {
                IModelDescription modelDesc = ModelCalibration.Instance.Calibrate(modelCalibDesc.Result, market.Result);
                return ModelFactory.Instance.Build(modelDesc, market.Result);
            }, market, modelCalibDesc);
        }
        private static Task<IPricingResult> ComputePrice(TaskFactory taskFactory, Task<IPricer> pricer, Task<IProduct> product, Task<IModel> model, Task<Market> market)
        {
            return taskFactory.ComputationTaskWithLog("Monte-Carlo simulation",
                () => pricer.Result.Price(product.Result, model.Result, market.Result)
                , product, model, market, pricer);
        }
        #endregion
        
        [ExcelFunction(Description = "Exotic product pricing function",
                       Category = "PragmaticQuant_Pricing")]
        public static object ExoticPrice(object requestObj, object[,] productBag, object mktObj, object[,] modelBag, object[,] algorithmBag)
        {
            return FunctionRunnerUtils.Run("ExoticPrice", () =>
            {
                Trace.WriteLine("");
                
                TaskFactory taskFactory = Task.Factory;
                Task<Market> market = BuildMarket(taskFactory, mktObj);
                Task<ICalibrationDescription> modelCalibDesc = BuildCalibration(taskFactory, modelBag);
                Task<IProduct> product = BuildProduct(taskFactory, productBag);
                
                Task<IModel> model = CalibrateModel(taskFactory, market, modelCalibDesc);
                Task<IPricer> pricer = BuildPricer(taskFactory, requestObj, algorithmBag);
                Task<IPricingResult> priceResult = ComputePrice(taskFactory, pricer, product, model, market);
                
                Trace.WriteLine("");
                return PriceResultPublisher.Instance.Publish(priceResult.Result);
            });
        }

    }
    
}