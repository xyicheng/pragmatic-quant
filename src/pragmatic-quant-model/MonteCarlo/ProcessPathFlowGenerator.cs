using pragmatic_quant_model.Maths.Stochastic;

namespace pragmatic_quant_model.MonteCarlo
{

    public class ProcessPathFlowGenerator<TPathFlow> : IPathGenerator<TPathFlow>
    {
        #region private fields
        private readonly IProcessPathGenerator processPathGen;
        private readonly IPathFlowCalculator<TPathFlow> flowPathCalculator;
        #endregion
        public ProcessPathFlowGenerator(IProcessPathGenerator processPathGen,
                                        IPathFlowCalculator<TPathFlow> flowPathCalculator)
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

    public interface IPathFlowCalculator<out TPathFlow>
    {
        TPathFlow Compute(IProcessPath processPath);
        int SizeOfPathInBits { get; }
    }
}