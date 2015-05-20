using System;
using System.Collections.Generic;
using System.Linq;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Model;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.MonteCarlo
{
    public class McModel
    {
        public McModel(IFactorModelRepresentation factorRepresentation,
            DateTime[] simulatedDates,
            IRandomGenerator randomGenerator,
            IProcessPathGenerator processPathGen,
            PaymentInfo probaMeasure, double numeraire0)
        {
            Numeraire0 = numeraire0;
            ProbaMeasure = probaMeasure;
            ProcessPathGen = processPathGen;
            RandomGenerator = randomGenerator;
            SimulatedDates = simulatedDates;
            FactorRepresentation = factorRepresentation;
        }

        public IFactorModelRepresentation FactorRepresentation { get; private set; }
        public DateTime[] SimulatedDates { get; private set; }
        public IRandomGenerator RandomGenerator { get; private set; }
        public IProcessPathGenerator ProcessPathGen { get; private set; }
        public PaymentInfo ProbaMeasure { get; private set; }
        public double Numeraire0 { get; private set; }
    }

    public class McModelFactory<TModel> where TModel : IModel
    {
        #region private fields
        private readonly IFactorRepresentationFactory<TModel> factorRepresentationFactory;
        private readonly IModelPathGenereratorFactory<TModel> modelPathGenFactory;
        private readonly IRandomGeneratorFactory randGeneratorFactory;
        #endregion
        #region private methods
        private PaymentInfo ProbaMeasure(IEnumerable<DateTime> simulatedDates, TModel model)
        {
            var horizon = simulatedDates.Max();
            return new PaymentInfo(model.PivotCurrency, horizon,
                FinancingId.RiskFree(model.PivotCurrency));
        }
        #endregion
        public McModelFactory(IFactorRepresentationFactory<TModel> factorRepresentationFactory, 
                              IModelPathGenereratorFactory<TModel> modelPathGenFactory, 
                              IRandomGeneratorFactory randGeneratorFactory)
        {
            this.factorRepresentationFactory = factorRepresentationFactory;
            this.modelPathGenFactory = modelPathGenFactory;
            this.randGeneratorFactory = randGeneratorFactory;
        }

        public McModel Build(TModel model, DateTime[] simulatedDates, Market market)
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

    public interface IModelPathGenereratorFactory<in TModel> where TModel : IModel
    {
        IProcessPathGenerator Build(TModel model, PaymentInfo probaMeasure, DateTime[] simulatedDates);
    }


}