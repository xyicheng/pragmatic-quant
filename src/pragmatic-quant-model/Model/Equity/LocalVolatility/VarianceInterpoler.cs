using System;
using System.Diagnostics.Contracts;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Function;

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
        public RrFunction TimeSlice(double t)
        {
            var linearIndex = maturityStepSearcher.LocateLeftIndex(t);

            if (linearIndex < 0)
            {
                return t * pillarVariances[0] / maturityPillars[0];
            }
            if (linearIndex == pillarVariances.Length - 1)
            {
                return t * pillarVariances[pillarVariances.Length - 1] / maturityPillars[pillarVariances.Length - 1];
            }

            double w = (t - maturityPillars[linearIndex]) / (maturityPillars[linearIndex + 1] - maturityPillars[linearIndex]);
            return (1.0 - w) * pillarVariances[linearIndex] + w * pillarVariances[linearIndex + 1];

        }
        public LocalVariance BuildLocalVariance()
        {
            return LocalVariance.Build(maturityPillars, pillarVariances);
        }
    }

    public static class SmileLawUtils
    {
        public static double Cumulative(double forward, double maturity, Func<double, double>  vol, double strike)
        {
            var v = vol(strike);
            var dk = strike * 1e-5;
            var skew = (vol(strike + dk) - vol(strike - dk)) / (2.0 * dk);
            return BlackScholesOption.PriceDigit(forward, strike, v, maturity, -1.0, skew);
        }
        public static double ComputeQuantile(double forward, double maturity, Func<double, double> vol, double proba)
        {
            double s = vol(forward) * Math.Sqrt(maturity);
            double kGuess = forward * Math.Exp(s * (NormalDistribution.FastCumulativeInverse(proba) + 0.5 * s));

            Func<double, double> cumErr = m => Cumulative(forward, maturity, vol, forward * Math.Exp(m)) - proba;
            
            double m1, m2;
            RootUtils.Bracket(cumErr , Math.Log(kGuess/forward), 0.0, out m1, out m2);
            var mQuantile = RootUtils.Brenth(cumErr, m1, m2, forward * 1.0e-5, DoubleUtils.MachineEpsilon);
            return forward * Math.Exp(mQuantile);
        }
    }

}