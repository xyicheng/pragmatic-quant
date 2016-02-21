using System;
using System.Diagnostics.Contracts;

namespace pragmatic_quant_model.Basic
{
    public class StepFunction<T>
    {
        #region private fields
        private readonly StepSearcher stepSearcher;
        private readonly T[] values;
        private readonly T leftValue;
        #endregion
        public StepFunction(double[] pillars, T[] values, T leftValue)
        {
            Contract.Requires(pillars.Length == values.Length);
            this.values = values;
            this.leftValue = leftValue;
            stepSearcher = new StepSearcher(pillars);
        }

        public double[] Pillars
        {
            get { return stepSearcher.Pillars; }
        }
        public T[] Values
        {
            get { return values; }
        }
        public T LeftValue
        {
            get { return leftValue;}
        }

        public T Eval(double x)
        {
            int stepIndex;
            return Eval(x, out stepIndex);
        }
        public T Eval(double x, out int stepIndex)
        {
            stepIndex = stepSearcher.LocateLeftIndex(x);

            if (stepIndex <= -1)
                return leftValue;

            return values[stepIndex];
        }

        public StepFunction<TVal> Map<TVal>(Func<T, TVal> f)
        {
            return new StepFunction<TVal>(stepSearcher.Pillars, values.Map(f), f(leftValue));
        }
    }
}