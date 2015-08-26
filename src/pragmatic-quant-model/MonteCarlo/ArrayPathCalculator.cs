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
        public ArrayPathCalculator(int[] datesIndexes, 
                                   TLabel[][] labels, 
                                   Func<double[], double>[][] factorEvaluators)
        {
            Contract.Requires(labels.Length == factorEvaluators.Length);

            this.datesIndexes = datesIndexes;
            this.labels = labels;
            this.factorEvaluators = factorEvaluators;
        }

        public void ComputeFlows(ref PathFlows<double[], TLabel[]> pathFlows, IProcessPath processPath)
        {
            for (int i = 0; i < datesIndexes.Length; i++)
            {
                Func<double[], double>[] funcs = factorEvaluators[i];
                double[] factor = processPath.GetProcessValue(datesIndexes[i]);

                double[] flows = pathFlows.Flows[i];
                for (int j = 0; j < flows.Length; j++)
                    flows[j] = funcs[j](factor);
            }
        }
        public PathFlows<double[], TLabel[]> NewPathFlow()
        {
            var flows = new double[datesIndexes.Length][];
            for (int i = 0; i < datesIndexes.Length; i++)
                flows[i] = new double[factorEvaluators[i].Length];
            return new PathFlows<double[], TLabel[]>(flows, labels);
        }
        public int SizeOfPath
        {
            get { return factorEvaluators.Select(f => f.Length).Sum() * sizeof(double); }
        }
    }
}