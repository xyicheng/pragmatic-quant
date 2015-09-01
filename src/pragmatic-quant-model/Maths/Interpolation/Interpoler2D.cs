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
}