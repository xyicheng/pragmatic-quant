using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Maths;

namespace pragmatic_quant_model.Model
{
    public interface INumericalMethodConfig { }

    public class MonteCarloConfig : INumericalMethodConfig
    {
        public MonteCarloConfig(int nbPaths, IRandomGeneratorFactory randomGenerator, Duration mcStep = null)
        {
            RandomGenerator = randomGenerator;
            NbPaths = nbPaths;
            McStep = mcStep;
        }
        public int NbPaths { get; private set; }
        public IRandomGeneratorFactory RandomGenerator { get; private set; }

        public Duration McStep { get; private set; }
    }
}