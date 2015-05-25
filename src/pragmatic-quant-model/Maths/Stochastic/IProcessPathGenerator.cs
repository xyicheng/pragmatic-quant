using System.Diagnostics.Contracts;

namespace pragmatic_quant_model.Maths.Stochastic
{
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