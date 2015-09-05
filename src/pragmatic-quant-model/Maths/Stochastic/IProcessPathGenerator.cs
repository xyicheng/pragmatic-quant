using System.Diagnostics.Contracts;
using System.Linq;

namespace pragmatic_quant_model.Maths.Stochastic
{
    [ContractClass(typeof (ProcessPathGenContract))]
    public interface IProcessPathGenerator
    {
        IProcessPath Path(double[][] dWs);
        double[] Dates { get; }
        int ProcessDim { get; }

        double[] AllSimulatedDates { get; }
    }

    [ContractClassFor(typeof(IProcessPathGenerator))]
    internal abstract class ProcessPathGenContract : IProcessPathGenerator
    {
        public IProcessPath Path(double[][] dWs)
        {
            Contract.Requires(dWs.Length == AllSimulatedDates.Length);
            Contract.Requires(dWs.All(dw => dw.Length == ProcessDim));
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
        public double[] AllSimulatedDates
        {
            get { return default(double[]); }
        }
        public int RandomDim { get { return default(int); } }
    }

}