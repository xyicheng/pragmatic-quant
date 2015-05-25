using System;
using System.Collections.Generic;
using System.Diagnostics;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Model;
using pragmatic_quant_model.MonteCarlo;
using pragmatic_quant_model.MonteCarlo.Product;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.Pricing
{
    public class McPricer
    {
        #region private fields
        private readonly IModel model;
        private readonly McModelFactory mcModelFactory;
        private readonly MonteCarloConfig mcConfig;
        #endregion
        #region private methods
        private McEngine<PathFlows<double, PaymentInfo>, PathFlows<double, PaymentInfo>> McEngine(IProduct product, McModel mcModel)
        {
            var productPathFlowCalculator = McProductPathFlowFactory.Build(product, mcModel);
            var mcModelPathFlowGen = new McModelPathFlowGenerator<PathFlows<double, PaymentInfo>, PaymentInfo[]>
                (mcModel.ProcessPathGen, productPathFlowCalculator);
            return  new McEngine<PathFlows<double, PaymentInfo>, PathFlows<double, PaymentInfo>>
                (mcModel.RandomGenerator, mcModelPathFlowGen, PriceFlowsAggregator.Value);
        }
        #endregion
        public McPricer(IModel model, McModelFactory mcModelFactory, MonteCarloConfig mcConfig)
        {
            this.model = model;
            this.mcModelFactory = mcModelFactory;
            this.mcConfig = mcConfig;
        }
        
        public PriceResult Price(IProduct product, Market market)
        {
            var simulatedDates = product.RetrieveEventDates();
            McModel mcModel = mcModelFactory.Build(model, market, simulatedDates);
            var mcEngine = McEngine(product, mcModel);
            
            Console.WriteLine("Starting Monte-Carlo simulation...");
            var time = new Stopwatch();
            time.Start();
            
            PathFlows<double, PaymentInfo> result = mcEngine.Run(mcConfig.NbPaths);

            time.Stop();
            Console.WriteLine(string.Format("Monte-Carlo elpased {0}", time.Elapsed));

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