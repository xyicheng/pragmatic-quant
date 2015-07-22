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

    public class MixedLinearInterpolation2D
    {
        #region private fields
        private readonly StepSearcher linearAxisSearcher;
        private readonly double[] linearPillars;
        private readonly RrFunction[] functions;
        private readonly RrFunction extrapolFunction;
        #endregion

        public MixedLinearInterpolation2D(double[] linearPillars, RrFunction[] functions, RrFunction extrapolFunction)
        {
            this.linearPillars = linearPillars;
            this.functions = functions;
            this.extrapolFunction = extrapolFunction;
            linearAxisSearcher = new StepSearcher(linearPillars);
        }
        
        public double Eval(double x, double y)
        {
            var linearIndex = linearAxisSearcher.LocateLeftIndex(x);
            var leftFunc = linearIndex < 0 ? extrapolFunction : functions[linearIndex];

            if (linearIndex == functions.Length - 1)
                return leftFunc.Eval(y);

            var rightFunc = functions[linearIndex + 1];

            double w = (x - linearPillars[linearIndex]) / (linearPillars[linearIndex + 1] - linearPillars[linearIndex]);
            return (1.0 - w) * leftFunc.Eval(y) + w * rightFunc.Eval(y);
        }

        public MixedLinearInterpolation2D DerivativeY()
        {
            var funcDerivatives = functions.Map(f => f.Derivative());
            return new MixedLinearInterpolation2D(linearPillars, funcDerivatives, extrapolFunction.Derivative());
        }
    }

}