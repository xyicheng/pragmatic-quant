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
        private readonly CubicSplineElement[][] cubicSplineElements;
        #endregion
        
        public MixedLinearSplineInterpoler(double[] linearPillars, double[] splinePillars, double[,] values)
            : base(new StepSearcher(linearPillars), new StepSearcher(splinePillars))
        {
            Contract.Requires(values.GetLength(0) == linearPillars.Length);
            Contract.Requires(values.GetLength(1) == splinePillars.Length);

            this.linearPillars = linearPillars;
            maxLinearPillarIndex = linearPillars.Length - 1;

            cubicSplineElements = Enumerable.Range(0, values.GetLength(0))
                                    .Select(i => CubicSplineUtils.BuildSpline(splinePillars, values.Row(i)))
                                    .ToArray();
        }
        
        public override double RawInterpol(int xLeftIndex, int yLeftIndex, double x, double y)
        {
            var h = y - YPillars[yLeftIndex];
            
            if (xLeftIndex < 0)
            {
                var cubic = cubicSplineElements[0][yLeftIndex];
                return cubic.A + h * (cubic.B + h * (cubic.C + h * cubic.D));
            }

            if (xLeftIndex == maxLinearPillarIndex)
            {
                var cubic = cubicSplineElements[maxLinearPillarIndex][yLeftIndex];
                return cubic.A + h * (cubic.B + h * (cubic.C + h * cubic.D));
            }

            var startCubic = cubicSplineElements[xLeftIndex][yLeftIndex];
            var startSplineValue = startCubic.A + h * (startCubic.B + h * (startCubic.C + h * startCubic.D));

            var endCubic = cubicSplineElements[xLeftIndex + 1][yLeftIndex];
            var endSplineValue = endCubic.A + h * (endCubic.B + h * (endCubic.C + h * endCubic.D));

            double w = (x - linearPillars[xLeftIndex]) / (linearPillars[xLeftIndex + 1] - linearPillars[xLeftIndex]);
            return (1.0 - w) * startSplineValue + w * endSplineValue;
        }
    }
}