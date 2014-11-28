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
        private readonly double[] yy;
        private readonly double[] y2;
        #endregion
        public CubicSplineInterpolation(double[] xv, double[] values, double yp1, double ypn)
        {
            abscissae = xv;
            yy = values;
            stepSearcher = new StepSearcher(xv);
            y2 = new double[values.Length];

            var u = new double[xv.Length];
            if (double.IsNaN(yp1))
            {//Natural Spline
                y2[0] = 0.0;
            }
            else
            {
                y2[0] = -0.5;
                u[0] = (3.0 / (xv[1] - xv[0])) * ((values[1] - values[0]) / (xv[1] - xv[0]) - yp1);
            }

            for (int i = 1; i < xv.Length - 1; i++)
            {
                double sig = (xv[i] - xv[i - 1]) / (xv[i + 1] - xv[i - 1]);
                double p = sig * y2[i - 1] + 2.0;
                y2[i] = (sig - 1.0) / p;
                u[i] = (values[i + 1] - values[i]) / (xv[i + 1] - xv[i]) - (values[i] - values[i - 1]) / (xv[i] - xv[i - 1]);
                u[i] = (6.0 * u[i] / (xv[i + 1] - xv[i - 1]) - sig * u[i - 1]) / p;
            }

            int n = values.Length;
            double qn, un;
            if (double.IsNaN(ypn))
            {//Natural Spline
                qn = 0.0;
                un = 0.0;
            }
            else
            {
                qn = 0.5;
                un = (3.0 / (xv[n - 1] - xv[n - 1])) * (ypn - (values[n - 1] - values[n - 2]) / (xv[n - 1] - xv[n - 2]));
            }
            y2[n - 1] = (un - qn * u[n - 2]) / (qn * y2[n - 2] + 1.0);
            for (int k = n - 2; k >= 0; k--)
            {
                y2[k] = y2[k] * y2[k + 1] + u[k];
            }
        }
        public override double Eval(double x)
        {
            int klo = stepSearcher.LocateLeftIndex(x);
            int khi = klo + 1;
            if (klo <= -1 || klo >= abscissae.Length - 1)
                throw new Exception("Extrapolation not allowed");

            double h = abscissae[khi] - abscissae[klo];
            if (DoubleUtils.EqualZero(h))
                throw new Exception("Bad input to routine spline");
            double a = (abscissae[khi] - x) / h;
            double b = (x - abscissae[klo]) / h;
            double y = a * yy[klo] + b * yy[khi]
                       + ((a * a * a - a) * y2[klo] + (b * b * b - b) * y2[khi]) * (h * h) / 6.0;
            return y;
        }
    }
}
