using pragmatic_quant_model.Maths;

namespace pragmatic_quant_model.Model
{
    public interface INumericalMethodConfig { }

    public class MonteCarloConfig : INumericalMethodConfig
    {
        public MonteCarloConfig(int nbPaths, IRandomGeneratorFactory randomGenerator)
        {
            RandomGenerator = randomGenerator;
            NbPaths = nbPaths;
        }
        public int NbPaths { get; private set; }
        public IRandomGeneratorFactory RandomGenerator { get; private set; }
    }
}