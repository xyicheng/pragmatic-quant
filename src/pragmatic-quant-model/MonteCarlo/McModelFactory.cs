using System;
using System.Collections.Generic;
using System.Linq;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Stochastic;
using pragmatic_quant_model.Model;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.MonteCarlo
{
    public class McModelFactory
    {
        #region private fields
        private readonly MonteCarloConfig mcConfig;
        #endregion
        #region private methods
        private PaymentInfo ProbaMeasure(IEnumerable<DateTime> simulatedDates, IModel model)
        {
            var horizon = simulatedDates.Max();
            return new PaymentInfo(model.PivotCurrency, horizon);
        }
        #endregion
        public McModelFactory(MonteCarloConfig mcConfig)
        {
            this.mcConfig = mcConfig;
        }

        public McModel Build(IModel model, Market market, DateTime[] simulatedDates)
        {
            PaymentInfo probaMeasure = ProbaMeasure(simulatedDates, model);

            var factorRepresentationFactory = FactorRepresentationFactories.For(model);
            IFactorModelRepresentation factorRepresentation = factorRepresentationFactory.Build(model, market, probaMeasure);

            var modelPathGenFactory = ModelPathGeneratorFactories.For(model, mcConfig);
            IProcessPathGenerator processPathGenerator = modelPathGenFactory.Build(model, market, probaMeasure, simulatedDates);

            BrownianBridge brownianBridge = BrownianBridge.Create(processPathGenerator.AllSimulatedDates);
            int randomDim = brownianBridge.GaussianSize(processPathGenerator.ProcessDim);
            IRandomGenerator randomGenerator = mcConfig.RandomGenerator.Build(randomDim);
            double numeraire0 = market.DiscountCurve(probaMeasure.Financing).Zc(probaMeasure.Date);
            
            return new McModel(factorRepresentation, simulatedDates, 
                               randomGenerator, brownianBridge, processPathGenerator,
                               probaMeasure, numeraire0);
        }
    }

}