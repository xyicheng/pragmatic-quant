using System.Collections.Generic;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Model;
using pragmatic_quant_model.MonteCarlo;
using pragmatic_quant_model.MonteCarlo.Product;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.Pricing
{
    public class McPricer : IPricer
    {
        #region private fields
        private readonly McModelFactory mcModelFactory;
        private readonly int nbPaths;
        #endregion
        #region private methods
        private McEngine<PathFlows<double, PaymentInfo>, PathFlows<double, PaymentInfo>> McEngine(IProduct product, McModel mcModel)
        {
            var productPathFlowCalculator = ProductPathFlowFactory.Build(product, mcModel);
            var processPathFlowGen = new ProcessPathFlowGenerator<double, PaymentInfo>
                (mcModel.BrownianBridge, mcModel.ProcessPathGen, productPathFlowCalculator);
            return new McEngine<PathFlows<double, PaymentInfo>, PathFlows<double, PaymentInfo>>
                (mcModel.RandomGenerator, processPathFlowGen, PriceFlowsAggregator.Value);
        }
        #endregion
        public McPricer(MonteCarloConfig mcConfig)
        {
            mcModelFactory = new McModelFactory(mcConfig);
            nbPaths = mcConfig.NbPaths;
        }

        public PriceResult Price(IProduct product, IModel model, Market market)
        {
            var simulatedDates = product.RetrieveEventDates();
            McModel mcModel = mcModelFactory.Build(model, market, simulatedDates);
            var mcEngine = McEngine(product, mcModel);
            
            PathFlows<double, PaymentInfo> result = mcEngine.Run(nbPaths);
            
            var refCurrency = product.Financing.Currency;
            var priceDetails = new Dictionary<PaymentInfo, Price>();
            var totalPrice = 0.0;
            for (int i = 0; i < result.Flows.Length; i++)
            {
                PaymentInfo paymentInfo = result.Labels[i];
                var price = new Price(result.Flows[i], paymentInfo.Currency);
                priceDetails.Add(paymentInfo, price);
                totalPrice += price.Convert(refCurrency, market).Value;
            }
            return new PriceResult(new Price(totalPrice, refCurrency), priceDetails);
        }
    }
}