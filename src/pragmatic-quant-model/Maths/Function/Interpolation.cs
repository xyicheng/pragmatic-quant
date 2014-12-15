using System;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.Maths.Function
{
    public class LinearInterpolation : RRFunction
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
    }
    
    public class CubicSplineInterpolation : RRFunction
    {
        #region private fields
        private readonly StepSearcher stepSearcher;
        private readonly double[] abscissae;
        private readonly double[] values;
        private readonly double[] secondDerivatives;
        #endregion
        public CubicSplineInterpolation(double[] abscissae, double[] values, double leftDerivative, double rightDerivative)
        {
            this.abscissae = abscissae;
            this.values = values;
            stepSearcher = new StepSearcher(abscissae);
            secondDerivatives = new double[values.Length];

            var u = new double[abscissae.Length];
            if (double.IsNaN(leftDerivative))
            {//Natural Spline
                secondDerivatives[0] = 0.0;
            }
            else
            {
                secondDerivatives[0] = -0.5;
                u[0] = (3.0 / (abscissae[1] - abscissae[0])) * ((values[1] - values[0]) / (abscissae[1] - abscissae[0]) - leftDerivative);
            }

            for (int i = 1; i < abscissae.Length - 1; i++)
            {
                double sig = (abscissae[i] - abscissae[i - 1]) / (abscissae[i + 1] - abscissae[i - 1]);
                double p = sig * secondDerivatives[i - 1] + 2.0;
                secondDerivatives[i] = (sig - 1.0) / p;
                u[i] = (values[i + 1] - values[i]) / (abscissae[i + 1] - abscissae[i]) - (values[i] - values[i - 1]) / (abscissae[i] - abscissae[i - 1]);
                u[i] = (6.0 * u[i] / (abscissae[i + 1] - abscissae[i - 1]) - sig * u[i - 1]) / p;
            }

            int n = values.Length;
            double qn, un;
            if (double.IsNaN(rightDerivative))
            {//Natural Spline
                qn = 0.0;
                un = 0.0;
            }
            else
            {
                qn = 0.5;
                un = (3.0 / (abscissae[n - 1] - abscissae[n - 1])) * (rightDerivative - (values[n - 1] - values[n - 2]) / (abscissae[n - 1] - abscissae[n - 2]));
            }
            secondDerivatives[n - 1] = (un - qn * u[n - 2]) / (qn * secondDerivatives[n - 2] + 1.0);
            for (int k = n - 2; k >= 0; k--)
            {
                secondDerivatives[k] = secondDerivatives[k] * secondDerivatives[k + 1] + u[k];
            }
        }
        public override double Eval(double x)
        {
            int leftIndex = stepSearcher.LocateLeftIndex(x);
            int rightIndex = leftIndex + 1;
            if (leftIndex <= -1 || rightIndex >= abscissae.Length)
                throw new Exception("Extrapolation not allowed");

            double h = abscissae[rightIndex] - abscissae[leftIndex];
            if (DoubleUtils.EqualZero(h))
                throw new Exception("Bad input to routine spline");
            double w1 = (abscissae[rightIndex] - x) / h;
            double w2 = (x - abscissae[leftIndex]) / h;
            double y = w1 * values[leftIndex] + w2 * values[rightIndex]
                       + ((w1 * w1 * w1 - w1) * secondDerivatives[leftIndex] + (w2 * w2 * w2 - w2) * secondDerivatives[rightIndex]) * (h * h) / 6.0;
            return y;
        }
    }
}
