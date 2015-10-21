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
        private static Task<IPricer> BuildPricer(TaskFactory taskFactory, object[,] algorithmBag)
        {
            return taskFactory.ComputationTaskWithLog("pricer preparation", () =>
            {
                INumericalMethodConfig algorithm = AlgorithmFactory.Instance.Build(algorithmBag);

                var mcConfig = algorithm as MonteCarloConfig;
                if (mcConfig != null)
                    return (IPricer) new McPricer(mcConfig);

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
            return FunctionRunnerUtils.Run("Price", () =>
            {
                Trace.WriteLine("");
                
                TaskFactory taskFactory = Task.Factory;
                Task<Market> market = BuildMarket(taskFactory, mktObj);
                Task<ICalibrationDescription> modelCalibDesc = BuildCalibration(taskFactory, modelBag);
                Task<IProduct> product = BuildProduct(taskFactory, productBag);
                
                Task<IModel> model = CalibrateModel(taskFactory, market, modelCalibDesc);
                Task<IPricer> pricer = BuildPricer(taskFactory, algorithmBag);
                Task<IPricingResult> priceResult = ComputePrice(taskFactory, pricer, product, model, market);
                
                Trace.WriteLine("");
                return PriceResultPublisher.Instance.Publish(priceResult.Result);
            });
        }
        
        #region Old Version
        /*
        [ExcelFunction(Description = "Exotic product pricing function",
                       Category = "PragmaticQuant_Pricing")]
        public static object McPrice(object requestObj, object[,] productBag, object mktObj, object[,] modelBag, object[,] algorithmBag)
        {
            return XlFunctionRunner.Run("Price", () =>
            {
                Trace.WriteLine("Start pricing preparation...");
                var timer = new Stopwatch();
                timer.Start();

                IProduct product = ProductFactory.Instance.Build(productBag);
                Market market = MarketManager.Instance.GetMarket(mktObj);
                ICalibrationDescription modelCalibDesc = ModelCalibrationFactory.Instance.Build(modelBag);
                INumericalMethodConfig algorithm = AlgorithmFactory.Instance.Build(algorithmBag);
                
                timer.Stop();
                Trace.WriteLine(String.Format("Pricing preparation done in {0} min {1} s {2} ms",
                    timer.Elapsed.Minutes, timer.Elapsed.Seconds, timer.Elapsed.Milliseconds));

                Trace.WriteLine("Start model calibration...");
                timer.Restart();

                IModelDescription modelDesc = ModelCalibration.Instance.Calibrate(modelCalibDesc, market);
                IModel model = ModelFactory.Instance.Build(modelDesc, market);

                timer.Stop();
                Trace.WriteLine(String.Format("Model calibration done in {0} min {1} s {2} ms",
                    timer.Elapsed.Minutes, timer.Elapsed.Seconds, timer.Elapsed.Milliseconds));

                Trace.WriteLine("Start Monte-Carlo simulation...");
                timer.Restart();

                IPricer pricer = new McPricer(algorithm as MonteCarloConfig);
                PriceResult priceResult = pricer.Price(product, model, market);

                timer.Stop();
                Trace.WriteLine(String.Format("Monte-Carlo simulation done in {0} min {1} s {2} ms",
                    timer.Elapsed.Minutes, timer.Elapsed.Seconds, timer.Elapsed.Milliseconds));
                Trace.WriteLine("");

                return PriceResultPublisher.Instance.Publish(priceResult);
            });
        }
        */
        #endregion
    }
    
}