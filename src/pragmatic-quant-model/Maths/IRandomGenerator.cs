using pragmatic_quant_model.Maths.Sobol;

namespace pragmatic_quant_model.Maths
{
    public interface IRandomGenerator
    {
        double[] Next();
        int Dim { get; }
    }

    public static class RandomGenerators
    {
        public static IRandomGenerator GaussianSobol(int dimension, SobolDirection sobolDirection)
        {
            return new GaussianSobolGenerator(new SobolRsg(dimension, sobolDirection));
        }
    }

    internal class GaussianSobolGenerator : IRandomGenerator
    {
        #region private fields
        private readonly SobolRsg sobol;
        private readonly int dim;
        #endregion
        public GaussianSobolGenerator(SobolRsg sobol)
        {
            this.sobol = sobol;
            dim = sobol.Dimension();
        }

        public double[] Next()
        {
            var uniforms = sobol.NextSequence();
            var gaussians = new double[dim];
            for (int i = 0; i < gaussians.Length; i++)
                gaussians[i] = NormalDistribution.FastCumulativeInverse(uniforms[i]);
            return gaussians;
        }
        public int Dim
        {
            get { return dim; }
        }
    }
}