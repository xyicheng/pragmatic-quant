using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths.Function;

namespace pragmatic_quant_model.Maths.Interpolation
{
    public class CubicSplineInterpoler : RrFunction
    {
        #region private fields
        private readonly StepFunction<CubicSplineElement> stepCubic;
        private readonly double[] abscissae;
        #endregion

        public CubicSplineInterpoler(double[] abscissae, double[] values,
            double leftDerivative = double.NaN, double rightDerivative = double.NaN)
        {
            this.abscissae = abscissae;

            var cubics = CubicSplineUtils.BuildSpline(abscissae, values, leftDerivative, rightDerivative);
            var rightExtrapol = cubics.Last();
            var leftExtrapol = cubics.First();
            cubics = cubics.Concat(new[] { rightExtrapol }).ToArray();

            stepCubic = new StepFunction<CubicSplineElement>(abscissae, cubics.Map(CubicSplineUtils.AsCubic), leftExtrapol.AsCubic());
        }

        public override double Eval(double x)
        {
            int leftIndex;
            var cubic = stepCubic.Eval(x, out leftIndex);
            var h = x - abscissae[Math.Max(0, leftIndex)];
            return cubic.A + h * (cubic.B + h * (cubic.C + h * cubic.D));
        }
    }

    public static class CubicSplineUtils
    {

        /// <summary>
        /// Build cubic spline elements. Algorithm from numerical recipes.
        /// 
        /// Left boundary condition  : 
        ///     spline left derivative match leftDerivative
        ///     when leftDerivative = double.NaN or is missing then "natural" spline condition (zero second order derivative)
        /// 
        /// Right boundary condition  : 
        ///     spline right derivative match leftDerivative
        ///     when rightDerivative = double.NaN or is missing then "natural" spline condition (zero second order derivative)  
        /// </summary>
        /// <param name="abscissae"></param>
        /// <param name="values"></param>
        /// <param name="leftDerivative">Left boundary condition, default is "natural" : zero second derivative </param>
        /// <param name="rightDerivative">Right boundary condition, default is "natural" : zero second derivative </param>
        /// <returns></returns>
        public static Polynomial[] BuildSpline(double[] abscissae, double[] values,
            double leftDerivative = double.NaN, double rightDerivative = double.NaN)
        {
            Contract.Requires(abscissae.Length == values.Length);

            var secondDerivatives = new double[values.Length];

            var u = new double[abscissae.Length];
            if (double.IsNaN(leftDerivative)) //Natural Spline
            {
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
                u[i] = (values[i + 1] - values[i]) / (abscissae[i + 1] - abscissae[i]) -
                       (values[i] - values[i - 1]) / (abscissae[i] - abscissae[i - 1]);
                u[i] = (6.0 * u[i] / (abscissae[i + 1] - abscissae[i - 1]) - sig * u[i - 1]) / p;
            }

            int n = values.Length;
            double qn, un;
            if (double.IsNaN(rightDerivative)) //Natural Spline
            {
                qn = 0.0;
                un = 0.0;
            }
            else
            {
                qn = 0.5;
                un = (3.0 / (abscissae[n - 1] - abscissae[n - 1])) *
                     (rightDerivative - (values[n - 1] - values[n - 2]) / (abscissae[n - 1] - abscissae[n - 2]));
            }
            secondDerivatives[n - 1] = (un - qn * u[n - 2]) / (qn * secondDerivatives[n - 2] + 1.0);
            for (int k = n - 2; k >= 0; k--)
            {
                secondDerivatives[k] = secondDerivatives[k] * secondDerivatives[k + 1] + u[k];
            }

            return Enumerable.Range(0, abscissae.Length - 1)
                .Select(i =>
                    StepInterpoler(abscissae[i + 1] - abscissae[i], values[i], values[i + 1], secondDerivatives[i], secondDerivatives[i + 1])
                ).ToArray();
        }

        public static Polynomial StepInterpoler(double step, double leftVal, double rightVal,
            double leftSecondDeriv, double rightSecondDeriv)
        {
            double a = leftVal;
            double b = (rightVal - leftVal) / step - step * leftSecondDeriv / 3.0 - step * rightSecondDeriv / 6.0;
            double c = leftSecondDeriv / 6.0 + leftSecondDeriv / 3.0;
            double d = (rightSecondDeriv - leftSecondDeriv) / step / 6.0;

            return new Polynomial(a, b, c, d);
        }

        public static CubicSplineElement AsCubic(this Polynomial polynom)
        {
            if (polynom.Degree > 3)
                throw new Exception("Not a cubic polynomial !");
            return new CubicSplineElement(polynom.Coeffs[0], polynom.Coeffs[1], polynom.Coeffs[2], polynom.Coeffs[3]);
        }
    }

    /// <summary>
    /// Cubic polynomial data :
    /// x -> A + B*x + C*x^2 + D*x^3 
    /// </summary>
    [DebuggerDisplay("{A} + {B}*x + {C}*x^2 + {D}*x^3")]
    public struct CubicSplineElement
    {
        public CubicSplineElement(double a, double b, double c, double d)
        {
            A = a;
            B = b;
            C = c;
            D = d;
        }
        public readonly double A;
        public readonly double B;
        public readonly double C;
        public readonly double D;
    }
}