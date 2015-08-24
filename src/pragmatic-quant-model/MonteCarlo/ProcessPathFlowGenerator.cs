using pragmatic_quant_model.Maths.Stochastic;

namespace pragmatic_quant_model.MonteCarlo
{

    public class ProcessPathFlowGenerator<TFlow, TLabel>
        : IPathGenerator<PathFlows<TFlow, TLabel>>
    {
        #region private fields
        private readonly IProcessPathGenerator processPathGen;
        private readonly IPathFlowCalculator<TFlow, TLabel> flowPathCalculator;
        #endregion
        public ProcessPathFlowGenerator(IProcessPathGenerator processPathGen,
                                        IPathFlowCalculator<TFlow, TLabel> flowPathCalculator)
        {
            this.processPathGen = processPathGen;
            this.flowPathCalculator = flowPathCalculator;
        }

        public PathFlows<TFlow, TLabel> ComputePath(double[] randoms)
        {
            IProcessPath processPath = processPathGen.Path(randoms);
            return flowPathCalculator.Compute(processPath);
        }
        public int SizeOfPathInBits
        {
            get { return flowPathCalculator.SizeOfPathInBits; }
        }
    }

    public interface IPathFlowCalculator<TFlow, TLabel>
    {
        PathFlows<TFlow, TLabel> Compute(IProcessPath processPath);
        int SizeOfPathInBits { get; }
    }

}