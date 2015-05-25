using pragmatic_quant_model.Maths.Sobol;

namespace pragmatic_quant_model.Model
{
    public interface INumericalMethodConfig { }

    public class MonteCarloConfig : INumericalMethodConfig
    {
        public MonteCarloConfig(int nbPaths, SobolDirection sobolDirection)
        {
            SobolDirection = sobolDirection;
            NbPaths = nbPaths;
        }
        public int NbPaths { get; private set; }
        public SobolDirection SobolDirection { get; private set; }
    }
}