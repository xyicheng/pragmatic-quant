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
        private readonly IFactorRepresentationFactory factorRepresentationFactory;
        private readonly IModelPathGenereratorFactory modelPathGenFactory;
        private readonly IRandomGeneratorFactory randGeneratorFactory;
        #endregion
        #region private methods
        private PaymentInfo ProbaMeasure(IEnumerable<DateTime> simulatedDates, IModel model)
        {
            var horizon = simulatedDates.Max();
            return new PaymentInfo(model.PivotCurrency, horizon, FinancingId.RiskFree(model.PivotCurrency));
        }
        #endregion
        public McModelFactory(IFactorRepresentationFactory factorRepresentationFactory, 
            IModelPathGenereratorFactory modelPathGenFactory, 
            IRandomGeneratorFactory randGeneratorFactory)
        {
            this.factorRepresentationFactory = factorRepresentationFactory;
            this.modelPathGenFactory = modelPathGenFactory;
            this.randGeneratorFactory = randGeneratorFactory;
        }

        public McModel Build(IModel model, Market market, DateTime[] simulatedDates)
        {
            PaymentInfo probaMeasure = ProbaMeasure(simulatedDates, model);
            double numeraire0 = market.DiscountCurve(probaMeasure.Financing).Zc(probaMeasure.Date);
            
            IFactorModelRepresentation factorRepresentation = factorRepresentationFactory.Build(model, market);
            IProcessPathGenerator processPathGenerator = modelPathGenFactory.Build(model, probaMeasure, simulatedDates);
            
            IRandomGenerator randomGenerator = randGeneratorFactory.Build(processPathGenerator.RandomDim);

            return new McModel(factorRepresentation,
                simulatedDates, randomGenerator, processPathGenerator,
                probaMeasure, numeraire0);
        }
    }
}