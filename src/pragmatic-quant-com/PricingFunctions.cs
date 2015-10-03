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
            Category = "PragmaticQuant_PricingFunctions")]
        public static object Price(object requestObj, object[,] productBag, object mktObj, object[,] modelBag, object[,] algorithmBag)
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

                IModelDescription modelDesc = ModelCalibrations.For(modelCalibDesc).Calibrate(modelCalibDesc, market);
                IModel model = ModelFactories.For(modelDesc).Build(modelDesc, market);

                timer.Stop();
                Trace.WriteLine(String.Format("Model calibration done in {0} min {1} s {2} ms",
                    timer.Elapsed.Minutes, timer.Elapsed.Seconds, timer.Elapsed.Milliseconds));

                Trace.WriteLine("Start Monte-Carlo simulation...");
                timer.Restart();

                IPricer pricer = McPricer.For(modelDesc, algorithm as MonteCarloConfig);
                PriceResult priceResult = pricer.Price(product, model, market);

                timer.Stop();
                Trace.WriteLine(String.Format("Monte-Carlo simulation done in {0} min {1} s {2} ms",
                    timer.Elapsed.Minutes, timer.Elapsed.Seconds, timer.Elapsed.Milliseconds));
                Trace.WriteLine("");

                return PriceResultPublisher.Instance.Publish(priceResult);
            });
        }
    }
    
    
}