using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths.Stochastic;

namespace pragmatic_quant_model.MonteCarlo
{

    public class ProcessPathFlowGenerator<TFlow, TLabel>
        : IPathGenerator<PathFlows<TFlow, TLabel>>
    {
        #region private fields
        private readonly BrownianBridge brownianBridge;
        private readonly IProcessPathGenerator processPathGen;
        private readonly IPathFlowCalculator<TFlow, TLabel> flowPathCalculator;
        #endregion
        public ProcessPathFlowGenerator(BrownianBridge brownianBridge,
                                        IProcessPathGenerator processPathGen,
                                        IPathFlowCalculator<TFlow, TLabel> flowPathCalculator)
        {
            this.brownianBridge = brownianBridge;
            this.processPathGen = processPathGen;
            this.flowPathCalculator = flowPathCalculator;
        }

        public void ComputePath(ref PathFlows<TFlow, TLabel> path, double[] gaussians)
        {
            //TODO perhaps use a pool instead of instanciate a new one
            var dWs = ArrayUtils.CreateJaggedArray<double>(brownianBridge.Dates.Length, 1);
            
            brownianBridge.FillPathIncrements(ref dWs, gaussians);
            IProcessPath processPath = processPathGen.Path(dWs);
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