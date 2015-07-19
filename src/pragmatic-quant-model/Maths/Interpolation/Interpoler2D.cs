using System;
using System.Diagnostics.Contracts;
using System.Linq;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.Maths.Interpolation
{
    public abstract class Interpoler2D
    {
        #region private fields
        private readonly StepSearcher xSearcher;
        private readonly StepSearcher ySearcher;
        #endregion
        protected Interpoler2D(StepSearcher xSearcher, StepSearcher ySearcher)
        {
            this.xSearcher = xSearcher;
            this.ySearcher = ySearcher;
        }

        public abstract double RawInterpol(int xLeftIndex, int yLeftIndex, double x, double y);
        public double Eval(double x, double y)
        {
            var xLeftIndex = xSearcher.LocateLeftIndex(x);
            var yLeftIndex = ySearcher.LocateLeftIndex(y);
            return RawInterpol(xLeftIndex, yLeftIndex, x, y);
        }

        public double[] XPillars
        {
            get { return xSearcher.Pillars; }
        }
        public double[] YPillars
        {
            get { return ySearcher.Pillars; }
        }
    }

    public class MixedLinearSplineInterpoler : Interpoler2D
    {
        #region private fields
        private readonly int maxLinearPillarIndex;
        private readonly double[] linearPillars;
        private readonly RationalFraction[][] polynomials;
        #endregion
        
        #region private methods
        private MixedLinearSplineInterpoler(double[] linearPillars, double[] splinePillars, RationalFraction[][] polynomials)
            : base(new StepSearcher(linearPillars), new StepSearcher(splinePillars))
        {
            Contract.Requires(linearPillars.Length == polynomials.Length);
            Contract.Requires(polynomials.All(a => a.Length == splinePillars.Length - 1));

            this.linearPillars = linearPillars;
            this.polynomials = polynomials;
            maxLinearPillarIndex = linearPillars.Length - 1;
        }
        #endregion

        public static MixedLinearSplineInterpoler Build(double[] linearPillars, double[] splinePillars, double[,] values)
        {
            Contract.Requires(values.GetLength(0) == linearPillars.Length);
            Contract.Requires(values.GetLength(1) == splinePillars.Length);

            var polynomials = EnumerableUtils.For(0, values.GetLength(0), i =>
            {
                var splines = CubicSplineUtils.BuildSpline(splinePillars, values.Row(i));
                return splines.Map(p => (RationalFraction) p);
            });
            return new MixedLinearSplineInterpoler(linearPillars, splinePillars, polynomials);
        }

        public override double RawInterpol(int xLeftIndex, int yLeftIndex, double x, double y)
        {
            var h = y - YPillars[yLeftIndex];
            
            if (xLeftIndex < 0)
            {
                var polynom = polynomials[0][yLeftIndex];
                return polynom.Eval(h);
            }

            if (xLeftIndex == maxLinearPillarIndex)
            {
                var polynom = polynomials[maxLinearPillarIndex][yLeftIndex];
                return polynom.Eval(h);
            }

            var startPolynom = polynomials[xLeftIndex][yLeftIndex];
            var endPolynom = polynomials[xLeftIndex + 1][yLeftIndex];
            
            double w = (x - linearPillars[xLeftIndex]) / (linearPillars[xLeftIndex + 1] - linearPillars[xLeftIndex]);
            return (1.0 - w) * startPolynom.Eval(h) + w * endPolynom.Eval(h);
        }
        public MixedLinearSplineInterpoler Apply(Func<RationalFraction, RationalFraction> f)
        {
            var mappedPolynoms = polynomials.Map(polys => polys.Map(f));
            return new MixedLinearSplineInterpoler(XPillars, YPillars, mappedPolynoms);
        }
    }
}