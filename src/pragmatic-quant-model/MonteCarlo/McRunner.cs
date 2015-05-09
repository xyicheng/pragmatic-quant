using System.Collections.Generic;

namespace pragmatic_quant_model.MonteCarlo
{
    public class McRunner<TPath, TResult>
    {
        #region private fields
        private readonly IRandomGenerator randomGenerator;
        private readonly IPathGenerator<TPath> pathCalculator;
        private readonly IPathResultAgregator<TPath, TResult> pathResultAggregator;
        #endregion
        public McRunner(IRandomGenerator randomGenerator,
                        IPathGenerator<TPath> pathCalculator,
                        IPathResultAgregator<TPath, TResult> pathResultAggregator)
        {
            this.randomGenerator = randomGenerator;
            this.pathCalculator = pathCalculator;
            this.pathResultAggregator = pathResultAggregator;
        }
        public TResult Run(int nbPaths)
        {
            var pathResults = new TPath[nbPaths];
            for (int i = 0; i < nbPaths; i++)
            {
                var randoms = randomGenerator.Next();
                pathResults[i] = pathCalculator.ComputePath(randoms);
            }
            var result = pathResultAggregator.Aggregate(pathResults);
            return result;
        }
    }

    public interface IPathResultAgregator<in TPathResult, out TResult>
    {
        TResult Aggregate(IEnumerable<TPathResult> paths);
    }

    public interface IRandomGenerator
    {
        double[] Next();
        int Dim { get; }
    }

    public interface IPathGenerator<out TPath>
    {
        TPath ComputePath(double[] randoms);
        int SizeOfPathInBits { get; }
    }
}
