using System;
using System.Collections.Generic;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Model;
using pragmatic_quant_model.MonteCarlo;
using pragmatic_quant_model.MonteCarlo.Product;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.Pricing
{
    using CouponPathFlows = PathFlows<double, CouponFlowLabel>;

    public class McPricer : IPricer
    {
        #region private fields
        private readonly McModelFactory mcModelFactory;
        private readonly IMcPricingResult pricingResult;
        private readonly int nbPaths;
        #endregion
        #region private methods
        private McEngine<CouponPathFlows, CouponPathFlows> McEngine(IProduct product, McModel mcModel)
        {
            var productPathFlowCalculator = ProductPathFlowFactory.Build(product, mcModel);
            var processPathFlowGen = new ProcessPathFlowGenerator<double, CouponFlowLabel>
                (mcModel.BrownianBridge, mcModel.ProcessPathGen, productPathFlowCalculator);
            return new McEngine<CouponPathFlows, CouponPathFlows>
                (mcModel.RandomGenerator, processPathFlowGen, new PriceFlowsAggregator<CouponFlowLabel>());
        }
        #endregion
        
        public McPricer(MonteCarloConfig mcConfig, IMcPricingResult pricingResult)
        {
            this.pricingResult = pricingResult;
            mcModelFactory = new McModelFactory(mcConfig);
            nbPaths = mcConfig.NbPaths;
        }
        public static McPricer WithDetails(MonteCarloConfig mcConfig)
        {
            return new McPricer(mcConfig, new McPricingResultWithDetails());
        }

        public IPricingResult Price(IProduct product, IModel model, Market market)
        {
            var simulatedDates = product.RetrieveEventDates();
            McModel mcModel = mcModelFactory.Build(model, market, simulatedDates);
            var mcEngine = McEngine(product, mcModel);

            PathFlows<double, CouponFlowLabel> result = mcEngine.Run(nbPaths);
            return pricingResult.PricingResult(result, product, market);
        }
    }

    public interface IMcPricingResult
    {
        IPricingResult PricingResult(CouponPathFlows result, IProduct product, Market market);
    }

    public class McPricingResultWithDetails : IMcPricingResult
    {
        public IPricingResult PricingResult(CouponPathFlows result, IProduct product, Market market)
        {
            var refCurrency = product.Financing.Currency;
            var priceDetails = new List<Tuple<string, PaymentInfo, Price>>();
            var totalPrice = 0.0;
            for (int i = 0; i < result.Flows.Length; i++)
            {
                CouponFlowLabel couponFlow = result.Labels[i];
                var price = new Price(result.Flows[i], couponFlow.Payment.Currency);
                priceDetails.Add(new Tuple<string, PaymentInfo, Price>(couponFlow.Label, couponFlow.Payment, price));
                totalPrice += price.Convert(refCurrency, market).Value;
            }
            return new PriceResult(new Price(totalPrice, refCurrency), priceDetails.ToArray());
        }
    }

}