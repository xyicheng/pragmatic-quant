using System;
using System.Diagnostics.Contracts;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths.Function;

namespace pragmatic_quant_model.Maths.Interpolation
{
    public class SplineInterpoler : RrFunction
    {
        #region private fields
        private readonly StepFunction<RationalFraction> stepSplines;
        private readonly double[] pillars;
        #endregion
        #region private methods
        private SplineInterpoler(StepFunction<RationalFraction> stepSplines)
        {
            this.stepSplines = stepSplines;
            pillars = stepSplines.Pillars;
        }
        #endregion

        public static SplineInterpoler BuildCubicSpline(double[] abscissae, double[] values,
            double leftDerivative = double.NaN, double rightDerivative = double.NaN)
        {
            var stepSplines = SplineUtils.BuildCubicSpline(abscissae, values, leftDerivative, rightDerivative);
            return new SplineInterpoler(stepSplines.Map(p => (RationalFraction) p));
        }

        public static SplineInterpoler BuildLinearSpline(double[] abscissae, double[] values,
            double leftExtrapolationSlope = 0.0, double rightExtrapolationSlope = 0.0)
        {
            var stepSplines = SplineUtils.BuildLinearInterpolation(abscissae, values, leftExtrapolationSlope, rightExtrapolationSlope);
            return new SplineInterpoler(stepSplines.Map(p => (RationalFraction)p));
        }

        public override double Eval(double x)
        {
            int leftIndex;
            var spline = stepSplines.Eval(x, out leftIndex);
            var h = x - pillars[Math.Max(0, leftIndex)];
            return spline.Eval(h);
        }
        public override RrFunction Derivative()
        {
            return new SplineInterpoler(stepSplines.Map(f => f.Derivative()));
        }
        public override RrFunction Inverse()
        {
            return new SplineInterpoler(stepSplines.Map(f => 1.0 / f));
        }
        public override RrFunction Add(RrFunction other)
        {
            var cst = other as ConstantRrFunction;
            if (cst != null)
                return Add(this, cst);

            var spline = other as SplineInterpoler;
            if (spline != null)
                return Add(this, spline);

            return base.Add(other);
        }
        public override RrFunction Mult(RrFunction other)
        {
            var cst = other as ConstantRrFunction;
            if (cst != null)
                return Mult(this, cst);

            var spline = other as SplineInterpoler;
            if (spline != null)
                return Mult(this, spline);

            return base.Mult(other);
        }
        
        #region private methods
        private static RationalFraction BasedExtrapol(StepFunction<RationalFraction> stepSplines, double firstPillar)
        {
            var extrapol = stepSplines.Eval(double.NegativeInfinity);
            var basedNum = extrapol.Num.TaylorDev(firstPillar - stepSplines.Pillars.First());
            var basedDenom = extrapol.Denom.TaylorDev(firstPillar - stepSplines.Pillars.First());
            return basedNum / basedDenom; 
        }
        private static RationalFraction BasedSpline(StepFunction<RationalFraction> stepSplines, double basePoint)
        {
            int stepIndex;
            var spline = stepSplines.Eval(basePoint, out stepIndex);
            var splineBasePoint = stepSplines.Pillars[Math.Max(0, stepIndex)];

            var basedNum = spline.Num.TaylorDev(basePoint - splineBasePoint);
            var basedDenom = spline.Denom.TaylorDev(basePoint - splineBasePoint);

            return basedNum / basedDenom;
        }
        private static RrFunction BinaryOp(SplineInterpoler left, SplineInterpoler right,
            Func<RationalFraction, RationalFraction, RationalFraction> binaryOp)
        {
            var mergedPillars = left.pillars.Union(right.pillars).OrderBy(p => p).ToArray();
            var binOpElems = mergedPillars.Select(p =>
            {
                var leftElem = BasedSpline(left.stepSplines, p);
                var rightElem = BasedSpline(right.stepSplines, p);
                return binaryOp(leftElem, rightElem);
            }).ToArray();

            var leftExtrapol = BasedExtrapol(left.stepSplines, mergedPillars.First());
            var rightExtrapol = BasedExtrapol(right.stepSplines, mergedPillars.First());
            var binOpExtrapol = binaryOp(leftExtrapol, rightExtrapol);

            var stepSplines= new StepFunction<RationalFraction>(mergedPillars, binOpElems, binOpExtrapol);
            return new SplineInterpoler(stepSplines);
        }
        #endregion
        public static RrFunction Add(SplineInterpoler left, SplineInterpoler right)
        {
            return BinaryOp(left, right, (l, r) => l + r);
        }
        public static RrFunction Add(SplineInterpoler left, ConstantRrFunction cst)
        {
            return new SplineInterpoler(left.stepSplines.Map(f => f + cst.Value));
        }
        public static RrFunction Mult(SplineInterpoler left, SplineInterpoler right)
        {
            return BinaryOp(left, right, (l, r) => l * r);
        }
        public static RrFunction Mult(SplineInterpoler left, ConstantRrFunction cst)
        {
            return new SplineInterpoler(left.stepSplines.Map(f => f * cst.Value));
        }
    }

    public static class SplineUtils
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
        public static Polynomial[] BuildCubicSplineInterpol(double[] abscissae, double[] values,
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

            return EnumerableUtils.For(0, abscissae.Length - 1, i =>
            {
                var step = abscissae[i + 1] - abscissae[i];
                return StepCubicInterpoler(step, values[i], values[i + 1], secondDerivatives[i], secondDerivatives[i + 1]);
            });
        }

        public static StepFunction<Polynomial> BuildCubicSpline(double[] abscissae, double[] values,
            double leftDerivative = double.NaN, double rightDerivative = double.NaN)
        {
            var cubics = BuildCubicSplineInterpol(abscissae, values, leftDerivative, rightDerivative);

            var lastStep = abscissae[abscissae.Length - 1] - abscissae[abscissae.Length - 2];
            var rightSlope = cubics.Last().Derivative().Eval(lastStep);
            var rightExtrapol = new Polynomial(values.Last(), rightSlope);

            var leftSlope = cubics.First().Derivative().Eval(0.0);
            var leftExtrapol = new Polynomial(values.First(), leftSlope);

            cubics = cubics.Concat(new[] { rightExtrapol }).ToArray();

            return new StepFunction<Polynomial>(abscissae, cubics, leftExtrapol);
        }

        public static StepFunction<Polynomial> BuildLinearInterpolation(double[] abscissae, double[] values,
            double leftExtrapolationSlope, double rightExtrapolationSlope)
        {
            var linearInterpol = EnumerableUtils.For(0, abscissae.Length - 1,
                i => StepLinearInterpoler(abscissae[i + 1] - abscissae[i], values[i], values[i + 1]));
            var rightElem = new Polynomial(values[values.Length - 1], rightExtrapolationSlope);
            var leftElem = new Polynomial(values[0], leftExtrapolationSlope);

            return new StepFunction<Polynomial>(abscissae, linearInterpol.Concat(new[] {rightElem}).ToArray(), leftElem);
        }

        public static Polynomial StepCubicInterpoler(double step, double leftVal, double rightVal,
            double leftSecondDeriv, double rightSecondDeriv)
        {
            double a = leftVal;
            double b = (rightVal - leftVal) / step - step * leftSecondDeriv / 3.0 - step * rightSecondDeriv / 6.0;
            double c = leftSecondDeriv / 6.0 + leftSecondDeriv / 3.0;
            double d = (rightSecondDeriv - leftSecondDeriv) / step / 6.0;

            return new Polynomial(a, b, c, d);
        }

        public static Polynomial StepLinearInterpoler(double step, double leftVal, double rightVal)
        {
            var slope = (rightVal - leftVal) / step;
            return new Polynomial(leftVal, slope);
        }
    }
}