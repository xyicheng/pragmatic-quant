using pragmatic_quant_model.Maths;

namespace pragmatic_quant_model.MonteCarlo
{
    public class McEngine<TPath, TResult>
    {
        #region private fields
        private readonly IRandomGenerator randomGenerator;
        private readonly IPathGenerator<TPath> pathGenerator;
        private readonly IPathResultAgregator<TPath, TResult> pathResultAggregator;
        #endregion
        public McEngine(IRandomGenerator randomGenerator,
                        IPathGenerator<TPath> pathGenerator,
                        IPathResultAgregator<TPath, TResult> pathResultAggregator)
        {
            this.randomGenerator = randomGenerator;
            this.pathGenerator = pathGenerator;
            this.pathResultAggregator = pathResultAggregator;
        }
        public TResult Run(int nbPaths)
        {
            var pathResults = new TPath[nbPaths];
            for (int i = 0; i < nbPaths; i++)
                pathResults[i] = pathGenerator.NewPath();
            
            for (int i = 0; i < nbPaths; i++)
            {
                var randoms = randomGenerator.Next();
                pathGenerator.ComputePath(ref pathResults[i], randoms);
            }
            var result = pathResultAggregator.Aggregate(pathResults);
            return result;
        }
    }
}
