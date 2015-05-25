using pragmatic_quant_model.Maths;

namespace pragmatic_quant_model.MonteCarlo
{
    public class McEngine<TPath, TResult>
    {
        #region private fields
        private readonly IRandomGenerator randomGenerator;
        private readonly IPathGenerator<TPath> pathCalculator;
        private readonly IPathResultAgregator<TPath, TResult> pathResultAggregator;
        #endregion
        public McEngine(IRandomGenerator randomGenerator,
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
}
