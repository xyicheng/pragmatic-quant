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

        public  void ComputePath(ref PathFlows<TFlow, TLabel> path, double[] randoms)
        {
            IProcessPath processPath = processPathGen.Path(randoms);
            flowPathCalculator.ComputeFlows(ref path, processPath);
        }
        
        public PathFlows<TFlow, TLabel> NewPath()
        {
            return flowPathCalculator.NewPathFlow();
        }
        public int SizeOfPath
        {
            get { return flowPathCalculator.SizeOfPath; }
        }
    }

    public interface IPathFlowCalculator<TFlow, TLabel>
    {
        void ComputeFlows(ref PathFlows<TFlow, TLabel> pathFlows, IProcessPath processPath);

        PathFlows<TFlow, TLabel> NewPathFlow();
        int SizeOfPath { get; }
    }

}