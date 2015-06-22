using pragmatic_quant_model.Maths.Stochastic;

namespace pragmatic_quant_model.MonteCarlo
{

    public class ProcessPathFlowGenerator<TPathFlow, TLabel> : IPathGenerator<TPathFlow>
    {
        #region private fields
        private readonly IProcessPathGenerator processPathGen;
        private readonly IPathFlowCalculator<TPathFlow, TLabel> flowPathCalculator;
        #endregion
        public ProcessPathFlowGenerator(IProcessPathGenerator processPathGen,
                                        IPathFlowCalculator<TPathFlow, TLabel> flowPathCalculator)
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

    public interface IPathFlowCalculator<out TPathFlow, out TLabel>
    {
        TPathFlow Compute(IProcessPath processPath);
        TLabel Labels { get; }
        int SizeOfPathInBits { get; }
    }
}