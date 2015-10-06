using System;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Stochastic;
using pragmatic_quant_model.Model;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.MonteCarlo
{
    public class McModel
    {
        public McModel(IFactorModelRepresentation factorRepresentation,
                       DateTime[] simulatedDates,
                       IRandomGenerator randomGenerator,
                       BrownianBridge brownianBridge,
                       IProcessPathGenerator processPathGen,
                       PaymentInfo probaMeasure, double numeraire0)
        {
            Numeraire0 = numeraire0;
            ProbaMeasure = probaMeasure;
            ProcessPathGen = processPathGen;
            RandomGenerator = randomGenerator;
            BrownianBridge = brownianBridge;
            SimulatedDates = simulatedDates;
            FactorRepresentation = factorRepresentation;
        }

        public IFactorModelRepresentation FactorRepresentation { get; private set; }
        
        public IRandomGenerator RandomGenerator { get; private set; }
        public BrownianBridge BrownianBridge { get; private set; }
        public IProcessPathGenerator ProcessPathGen { get; private set; }
        
        public DateTime[] SimulatedDates { get; private set; }
        public PaymentInfo ProbaMeasure { get; private set; }
        public double Numeraire0 { get; private set; }
    }
}