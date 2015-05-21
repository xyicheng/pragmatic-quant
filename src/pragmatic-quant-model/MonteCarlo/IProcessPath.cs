using System.Diagnostics;
using System.Linq;

namespace pragmatic_quant_model.MonteCarlo
{
    public interface IProcessPath
    {
        double[] GetProcessValue(int dateIndex);
        double[] Dates { get; }
        int Dimension { get; }
    }

    [DebuggerDisplay("ProcessPath Dim={Dimension}")]
    public class ProcessPath : IProcessPath
    {
        #region private fields
        private readonly double[][] values;
        #endregion
        public ProcessPath(double[] dates, int dimension, double[][] values)
        {
            Debug.Assert(values.Length == dates.Length, "values.Length != dimension");
            Debug.Assert(values.Select(v => v.Length).Single() == dimension);
            this.values = values;
            Dates = dates;
            Dimension = dimension;
        }
        public double[] GetProcessValue(int dateIndex)
        {
            return values[dateIndex];
        }
        public double[] Dates { get; private set; }
        public int Dimension { get; private set; }
    }
}