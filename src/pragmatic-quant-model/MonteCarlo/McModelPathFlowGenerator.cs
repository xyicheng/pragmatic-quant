using System.Diagnostics.Contracts;

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

    public interface IProcessPath
    {
        double[] GetProcessValue(int dateIndex); //Faire un contract pour l'indice et la taille du tableaux
        double[] Dates { get; }
        int Dimension { get; }
    }

    [ContractClass(typeof(ProcessPathGenContract))]
    public interface IProcessPathGenerator
    {
        IProcessPath Path(double[] randoms);
        double[] Dates { get; }
        int ProcessDim { get; }
        int RandomDim { get; }
    }

    [ContractClassFor(typeof(IProcessPathGenerator))]
    internal abstract class ProcessPathGenContract : IProcessPathGenerator
    {
        public IProcessPath Path(double[] randoms)
        {
            Contract.Requires(randoms.Length == RandomDim);
            return null;
        }
        public double[] Dates
        {
            get { return null; }
        }
        public int ProcessDim
        {
            get { return default(int); }
        }
        public int RandomDim { get { return default(int); } }
    }
}