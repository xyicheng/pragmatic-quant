using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.Maths.Function
{
    public class StepFunction : RRFunction
    {
        #region private fields
        private readonly StepSearcher stepSearcher;
        private readonly double[] abscissae;
        private readonly double[] values;
        private readonly double leftValue;
        #endregion
        public StepFunction(double[] abscissae, double[] values, double leftValue)
        {
            this.abscissae = abscissae;
            this.values = values;
            this.leftValue = leftValue;
            stepSearcher = new StepSearcher(abscissae);
        }
        public override double Eval(double x)
        {
            int leftIndex = stepSearcher.LocateLeftIndex(x);

            if (leftIndex <= -1)
                return leftValue;

            return values[System.Math.Min(leftIndex, abscissae.Length - 1)];
        }
    }
}