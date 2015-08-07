using System;
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
            try
            {
                IProduct product = ProductFactory.Instance.Build(productBag);
                Market market = MarketManager.Instance.GetMarket(mktObj);
                ICalibrationDescription modelCalibDesc = ModelCalibrationFactory.Instance.Build(modelBag);
                INumericalMethodConfig algorithm = AlgorithmFactory.Instance.Build(algorithmBag);

                IModelDescription modelDesc = ModelCalibrations.For(modelCalibDesc).Calibrate(modelCalibDesc, market);
                IModel model = ModelFactories.For(modelDesc).Build(modelDesc, market);
                
                IPricer pricer = McPricer.For(modelDesc, algorithm as MonteCarloConfig);
                PriceResult priceResult = pricer.Price(product, model, market);

                return PriceResultPublisher.Instance.Publish(priceResult);
            }
            catch (Exception e)
            {
                var error = new object[1, 1];
                error[0, 0] = string.Format("ERROR, {0}", e.Message);
                return error;
            }
        }
    }

}