using System;
using System.Diagnostics.Contracts;
using System.Linq;
using pragmatic_quant_model.Maths.Stochastic;

namespace pragmatic_quant_model.MonteCarlo
{
    public class ArrayPathCalculator<TLabel>
        : IPathFlowCalculator<double[], TLabel[]>
    {
        #region private fields
        private readonly int[] datesIndexes;
        private readonly TLabel[][] labels;
        private readonly Func<double[], double>[][] factorEvaluators;
        #endregion
        #region private methods
        private double[][] ValuesBuffer()
        { 
            //TODO perhaps use a pool instead of instanciate a new one
            var values = new double[datesIndexes.Length][];
            for (int i = 0; i < datesIndexes.Length; i++)
                values[i] = new double[factorEvaluators[i].Length];
            return values;
        }

        #endregion
        public ArrayPathCalculator(int[] datesIndexes, 
                                   TLabel[][] labels, 
                                   Func<double[], double>[][] factorEvaluators)
        {
            Contract.Requires(labels.Length == factorEvaluators.Length);

            this.datesIndexes = datesIndexes;
            this.labels = labels;
            this.factorEvaluators = factorEvaluators;
        }
        
        public PathFlows<double[], TLabel[]> Compute(IProcessPath processPath)
        {
            var values = ValuesBuffer();
            for (int i = 0; i < datesIndexes.Length; i++)
            {
                Func<double[], double>[] funcs = factorEvaluators[i];
                double[] val = values[i];
                
                double[] factor = processPath.GetProcessValue(datesIndexes[i]);
                for (int j = 0; j < val.Length; j++)
                    val[j] = funcs[j](factor);
            }
            
            return new PathFlows<double[], TLabel[]>(values, labels);
        }
        public int SizeOfPathInBits
        {
            get { return factorEvaluators.Select(f => f.Length).Sum() * sizeof(double); }
        }
    }
}