using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths.Function;

namespace pragmatic_quant_model.Maths.Interpolation
{
    public class LinearInterpolation : RrFunction
    {
        #region private fields
        private readonly StepSearcher stepSearcher;
        private readonly double[] abscissae;
        private readonly double[] values;
        private readonly double leftExtrapolationSlope;
        private readonly double rightExtrapolationSlope;
        #endregion
        public LinearInterpolation(double[] abscissae, double[] values, double leftExtrapolationSlope, double rightExtrapolationSlope)
        {
            this.abscissae = abscissae;
            this.values = values;
            this.leftExtrapolationSlope = leftExtrapolationSlope;
            this.rightExtrapolationSlope = rightExtrapolationSlope;
            stepSearcher = new StepSearcher(abscissae);
        }
        public override double Eval(double x)
        {
            int leftIndex = stepSearcher.LocateLeftIndex(x);

            if (leftIndex <= -1)
                return values[0] + (x - abscissae[0]) * leftExtrapolationSlope;

            if (leftIndex >= abscissae.Length - 1)
                return values[abscissae.Length - 1] + (x - abscissae[abscissae.Length - 1]) * rightExtrapolationSlope;

            double step = abscissae[leftIndex + 1] - abscissae[leftIndex];
            double slope = (step > 0.0)
                ? (values[leftIndex + 1] - values[leftIndex]) / (abscissae[leftIndex + 1] - abscissae[leftIndex])
                : 0.0;
            return values[leftIndex] + (x - abscissae[leftIndex]) * slope;
        }
        public override RrFunction Add(RrFunction other)
        {
            var cst = other as ConstantRrFunction;
            if (cst != null)
            {
                var shiftedValues = values.Map(v => v + cst.Value);
                return new LinearInterpolation(abscissae, shiftedValues, leftExtrapolationSlope, rightExtrapolationSlope);
            }

            var linear = other as LinearInterpolation;
            if (linear != null)
            {
                var mergedAbscissae = abscissae.Union(linear.abscissae).OrderBy(p => p).ToArray();
                var addValues = mergedAbscissae.Map(p => Eval(p) + linear.Eval(p));
                return new LinearInterpolation(mergedAbscissae, addValues,
                    leftExtrapolationSlope + linear.leftExtrapolationSlope,
                    rightExtrapolationSlope + linear.rightExtrapolationSlope);
            }

            return base.Add(other);
        }
    }
}
