using System;
using System.Diagnostics;
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

        #region work in progress
        /*
        [ExcelFunction(Description = "Exotic product pricing function",
            Category = "PragmaticQuant_PricingFunctions")]
        public static object Price(object requestObj, object[,] productBag, object mktObj, object[,] modelBag, object[,] algorithmBag)
        {
            return XlFunctionRunner.Run("Price", () =>
            {
                Trace.WriteLine("");
                TaskFactory f = Task.Factory;
                var market = f.ComputationTaskWithLog("market preparation", () => MarketManager.Instance.GetMarket(mktObj));
                var modelCalibDesc = f.ComputationTaskWithLog("model preparation", () => ModelCalibrationFactory.Instance.Build(modelBag));
                var pricer = f.ComputationTaskWithLog("pricer preparation", () =>
                {
                    var algorithm = AlgorithmFactory.Instance.Build(algorithmBag);
                    return new McPricer(algorithm as MonteCarloConfig);
                });
                
                var product = f.ComputationTaskWithLog("Product preparation", 
                    () => ProductFactory.Instance.Build(productBag));

                var model = f.ComputationTaskWithLog("Model calibration", () =>
                {
                    IModelDescription modelDesc = ModelCalibration.Instance.Calibrate(modelCalibDesc.Result, market.Result);
                    return ModelFactory.Instance.Build(modelDesc, market.Result);
                }, market, modelCalibDesc);

                var priceResult = f.ComputationTaskWithLog("Monte-Carlo simulation",
                    () => pricer.Result.Price(product.Result, model.Result, market.Result), product, model, market, pricer);
                
                Trace.WriteLine("");
                return PriceResultPublisher.Instance.Publish(priceResult.Result);
            });
        }
        */
        #endregion
    }
    
    
}