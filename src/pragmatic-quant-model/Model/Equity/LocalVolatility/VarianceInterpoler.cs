using System;
using System.Diagnostics.Contracts;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths;

namespace pragmatic_quant_model.Model.Equity.LocalVolatility
{
    public class VarianceInterpoler
    {
        #region private fields
        private readonly StepSearcher maturityStepSearcher;
        private readonly double[] maturityPillars;
        private readonly RrFunction[] pillarVariances;
        #endregion

        public VarianceInterpoler(double[] maturityPillars, RrFunction[] pillarVariances)
        {
            Contract.Requires(maturityPillars.All(t => t > 0.0));

            this.maturityPillars = maturityPillars;
            this.pillarVariances = pillarVariances;
            maturityStepSearcher = new StepSearcher(maturityPillars);
        }

        public double Eval(double t, double y)
        {
            var linearIndex = maturityStepSearcher.LocateLeftIndex(t);
            
            if (linearIndex < 0)
            {
                return t * pillarVariances[0].Eval(y) / maturityPillars[0];
            }
            if (linearIndex == pillarVariances.Length - 1)
            {
                return t * pillarVariances[pillarVariances.Length - 1].Eval(y) / maturityPillars[pillarVariances.Length - 1];
            }
            
            double w = (t - maturityPillars[linearIndex]) / (maturityPillars[linearIndex + 1] - maturityPillars[linearIndex]);
            return (1.0 - w) * pillarVariances[linearIndex].Eval(y) + w * pillarVariances[linearIndex + 1].Eval(y);
        }
        public LocalVariance BuildLocalVariance()
        {
            return LocalVariance.Build(maturityPillars, pillarVariances);
        }

    }

    public class LocalVariance
    {
        #region private fields
        private static readonly RrFunction m = RrFunctions.Affine(1.0, 0.0);
        private readonly StepSearcher maturityStepSearcher;
        private readonly double[] maturities;
        private readonly RrFunction[] vs;
        private readonly RrFunction[] dVdYs;
        private readonly RrFunction[] d2Vd2Ys;
        private readonly RrFunction[] dVdTs;
        private readonly RrFunction shortLocalVar;
        private readonly RrFunction[] pillarStepIntegrals;
        #endregion
        #region private methods
        private RrFunction StepQuadrature(double start, double end)
        {
            var dt = (end - start);
            if (dt > 0.0)
            {
                //2 points Gauss-Legendre
                return (0.5 * dt) * (TimeSlice(start + dt * 0.211324865405187) + TimeSlice(start + dt * 0.788675134594813));
                //return (0.5 * dt) * (TimeSlice(start * (1.0 + 1e-12)) + TimeSlice(end * (1.0 - 1e-12)));
            }
            return RrFunctions.Zero;
        }
        private RrFunction GetPillarStepQuadrature(int index)
        {
            if (pillarStepIntegrals[index] != null)
                return pillarStepIntegrals[index];

            if (index == maturities.Length - 1)
                throw new Exception("should never get there !");

            var stepAvg = StepQuadrature(maturities[index], maturities[index + 1]);
            pillarStepIntegrals[index] = stepAvg;
            return stepAvg;
        }
        private LocalVariance(double[] maturities, RrFunction[] vs, RrFunction[] dVdYs, RrFunction[] d2Vd2Ys, RrFunction[] dVdTs)
        {
            Contract.Requires(maturities.All(t => t > 0.0));

            this.maturities = maturities;
            this.vs = vs;
            this.dVdYs = dVdYs;
            this.d2Vd2Ys = d2Vd2Ys;
            this.dVdTs = dVdTs;
            maturityStepSearcher = new StepSearcher(maturities);
            pillarStepIntegrals = new RrFunction[maturities.Length - 1];
            
            //                                  sigma^2
            // short local variance = -------------------------------------------
            //                         (y * (dsigma^2/dy) / (2 * sigma^2) - 1)^2 
            var sigma2 = vs[0] / maturities[0];
            shortLocalVar = 0.5 * m * sigma2.Derivative() / sigma2 - 1.0;
            shortLocalVar = sigma2 / (shortLocalVar * shortLocalVar);
        }
        #endregion

        public static LocalVariance Build(double[] maturityPillars, RrFunction[] pillarVariances)
        {
            RrFunction[] dVdTs = EnumerableUtils.For(0, pillarVariances.Length, i =>
            {
                if (i == pillarVariances.Length - 1)
                    return pillarVariances[i] / (maturityPillars[i]);

                var step = maturityPillars[i + 1] - maturityPillars[i];
                return (pillarVariances[i + 1] - pillarVariances[i]) / step;
            });
            var dVdYs = pillarVariances.Map(v => v.Derivative());
            var d2Vd2Ys = dVdYs.Map(dvdy => dvdy.Derivative());

            return new LocalVariance(maturityPillars, pillarVariances, dVdYs, d2Vd2Ys, dVdTs);
        }

        public double Eval(double t, double y)
        {
            var stepIndex = maturityStepSearcher.LocateLeftIndex(t);
            double v, dVdY, d2Vd2Y, dVdT;
            if (stepIndex < 0)
            {
                double w = t / maturities[0];

                if (Math.Abs(w) < 1.0e-10)
                    return shortLocalVar.Eval(y);

                v = w * vs[0].Eval(y);
                dVdY = w * dVdYs[0].Eval(y);
                d2Vd2Y = w * d2Vd2Ys[0].Eval(y);
                dVdT = vs[0].Eval(y) / maturities[0];
            }
            else if (stepIndex == maturities.Length - 1)
            {
                double w = t / maturities[maturities.Length - 1];
                v = vs[stepIndex].Eval(y) * w;
                dVdY = dVdYs[stepIndex].Eval(y) * w;
                d2Vd2Y = d2Vd2Ys[stepIndex].Eval(y) * w;
                dVdT = dVdTs[stepIndex].Eval(y);
            }
            else
            {
                double w = (t - maturities[stepIndex]) / (maturities[stepIndex + 1] - maturities[stepIndex]);
                v = (1.0 - w) * vs[stepIndex].Eval(y) + w * vs[stepIndex + 1].Eval(y);
                dVdY = (1.0 - w) * dVdYs[stepIndex].Eval(y) + w * dVdYs[stepIndex + 1].Eval(y);
                d2Vd2Y = (1.0 - w) * d2Vd2Ys[stepIndex].Eval(y) + w * d2Vd2Ys[stepIndex + 1].Eval(y);
                dVdT = dVdTs[stepIndex].Eval(y);
            }
            
            //                                                  dv/dt 
            // local variance = ------------------------------------------------------------------------------------
            //                   (y * (dv/dy) / (2 * v) - 1)^2 + 1/2 * (d^2 v/dy^2) - 1/4 * (1/4 + 1/v) * (dv/dy)^2
            
            double localVar = 0.5 * y * dVdY / v - 1.0;
            localVar = localVar * localVar + 0.5 * d2Vd2Y - 0.25 * (0.25 + 1.0 / v) * dVdY * dVdY;
            localVar = dVdT / localVar;
            return localVar;
        }
        public RrFunction TimeSlice(double t)
        {
            var stepIndex = maturityStepSearcher.LocateLeftIndex(t);
            RrFunction v, dVdY, d2Vd2Y, dVdT;
            if (stepIndex < 0)
            {
                double w = t / maturities[0];

                if (Math.Abs(w) < 1.0e-10)
                    return shortLocalVar;

                v = w * vs[0];
                dVdY = w * dVdYs[0];
                d2Vd2Y = w * d2Vd2Ys[0];
                dVdT = vs[0] / maturities[0];
            }
            else if (stepIndex == maturities.Length - 1)
            {
                double w = t / maturities[maturities.Length - 1];
                v = vs[stepIndex] * w;
                dVdY = dVdYs[stepIndex] * w;
                d2Vd2Y = d2Vd2Ys[stepIndex] * w;
                dVdT = dVdTs[stepIndex];
            }
            else
            {
                double w = (t - maturities[stepIndex]) / (maturities[stepIndex + 1] - maturities[stepIndex]);
                v = (1.0 - w) * vs[stepIndex] + w * vs[stepIndex + 1];
                dVdY = (1.0 - w) * dVdYs[stepIndex] + w * dVdYs[stepIndex + 1];
                d2Vd2Y = (1.0 - w) * d2Vd2Ys[stepIndex] + w * d2Vd2Ys[stepIndex + 1];
                dVdT = dVdTs[stepIndex];
            }

            //                                                  dv/dt 
            // local variance = ------------------------------------------------------------------------------------
            //                   (y * (dv/dy) / (2 * v) - 1)^2 + 1/2 * (d^2 v/dy^2) - 1/4 * (1/4 + 1/v) * (dv/dy)^2
            var localVar = 0.5 * m * dVdY / v - 1.0;
            localVar = localVar * localVar + 0.5 * d2Vd2Y - 0.25 * (0.25 + 1.0 / v) * dVdY * dVdY;
            localVar = dVdT / localVar;
            return localVar;
        }
        public RrFunction TimeAverage(double start, double end)
        {
            if (start > end)
                throw new Exception("LocalVariance.TimeAverage : start must be lower than end");

            int startIndex = maturityStepSearcher.LocateLeftIndex(start);
            int endIndex = maturityStepSearcher.LocateLeftIndex(end);

            if (startIndex == endIndex)
                return StepQuadrature(start, end) / (end - start);

            var average = StepQuadrature(start, maturities[startIndex + 1]);
            for (int i = startIndex + 1; i < endIndex; i++)
                average += GetPillarStepQuadrature(i);
            average += StepQuadrature(maturities[endIndex], end);
            average /= (end - start);
            return average;
        }
    }

}