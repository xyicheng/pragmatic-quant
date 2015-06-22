using System;
using System.Linq;
using pragmatic_quant_model.Maths.Stochastic;

namespace pragmatic_quant_model.MonteCarlo
{
    public class ArrayPathCalculator<TLabel>
        : IPathFlowCalculator<PathFlows<double[], TLabel[]>, TLabel[][]>
    {
        #region private fields
        private readonly int[] datesIndexes;
        private readonly TLabel[][] labels;
        private readonly Func<double[], double>[][] factorEvaluators;
        #endregion
        public ArrayPathCalculator(int[] datesIndexes, 
                                    TLabel[][] labels, 
                                    Func<double[], double>[][] factorEvaluators)
        {
            this.datesIndexes = datesIndexes;
            this.labels = labels;
            this.factorEvaluators = factorEvaluators;
        }
        
        public PathFlows<double[], TLabel[]> Compute(IProcessPath processPath)
        {
            var values = new double[datesIndexes.Length][];
            for (int i = 0; i < datesIndexes.Length; i++)
            {
                var value = processPath.GetProcessValue(datesIndexes[i]);
                values[i] = factorEvaluators[i].Select(f => f(value)).ToArray();
            }
            return new PathFlows<double[], TLabel[]>(values, labels);
        }
        public TLabel[][] Labels
        {
            get { return labels; }
        }
        public int SizeOfPathInBits
        {
            get { return labels.Select(f => f.Length).Sum() * sizeof(double); }
        }
    }
}