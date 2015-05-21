namespace pragmatic_quant_model.MonteCarlo
{

    public class McModelPathFlowGenerator<TPathFlow, TLabel> : IPathGenerator<TPathFlow>
    {
        #region private fields
        private readonly IProcessPathGenerator processPathGen;
        private readonly IMcPathFlowCalculator<TPathFlow, TLabel> flowPathCalculator;
        #endregion
        public McModelPathFlowGenerator(IProcessPathGenerator processPathGen,
                                        IMcPathFlowCalculator<TPathFlow, TLabel> flowPathCalculator)
        {
            this.processPathGen = processPathGen;
            this.flowPathCalculator = flowPathCalculator;
        }

        public TPathFlow ComputePath(double[] randoms)
        {
            var processPath = processPathGen.Path(randoms);
            return flowPathCalculator.Compute(processPath);
        }
        public int SizeOfPathInBits
        {
            get { return flowPathCalculator.SizeOfPathInBits; }
        }
    }

    public interface IMcPathFlowCalculator<out TPathFlow, out TLabel>
    {
        TPathFlow Compute(IProcessPath processPath);
        TLabel Labels { get; }
        int SizeOfPathInBits { get; }
    }
}