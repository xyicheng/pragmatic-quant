using System.Diagnostics.Contracts;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths;

namespace pragmatic_quant_model.Model.LocalVolatility
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
            Contract.Requires(maturityPillars.All(t => t >= 0.0));

            this.maturityPillars = maturityPillars;
            this.pillarVariances = pillarVariances;
            maturityStepSearcher = new StepSearcher(maturityPillars);
        }

        public double Eval(double t, double y)
        {
            var linearIndex = maturityStepSearcher.LocateLeftIndex(t);
            RrFunction leftVariance = linearIndex < 0 ? RrFunctions.Zero : pillarVariances[linearIndex];

            if (linearIndex == pillarVariances.Length - 1)
            {
                return t * leftVariance.Eval(y) / maturityPillars[linearIndex];
            }

            var rightVariance = pillarVariances[linearIndex + 1];

            double w = (t - maturityPillars[linearIndex]) / (maturityPillars[linearIndex + 1] - maturityPillars[linearIndex]);
            return (1.0 - w) * leftVariance.Eval(y) + w * rightVariance.Eval(y);
        }
        public LocalVariance BuildLocalVariance()
        {
            return LocalVariance.Build(maturityPillars, pillarVariances);
        }

    }

    public class LocalVariance
    {
        #region private fields
        private readonly StepSearcher maturityStepSearcher;
        private readonly double[] maturityPillars;
        private readonly RrFunction[] varianceTimeDerivatives;
        private readonly RrFunction[] localVarianceDenominators;
        #endregion
        #region private methods
        private LocalVariance(double[] maturityPillars, RrFunction[] varianceTimeDerivatives, RrFunction[] localVarianceDenominators)
        {
            Contract.Requires(maturityPillars.All(t => t >= 0.0));

            this.maturityPillars = maturityPillars;
            this.varianceTimeDerivatives = varianceTimeDerivatives;
            this.localVarianceDenominators = localVarianceDenominators;
            maturityStepSearcher = new StepSearcher(maturityPillars);
        }
        #endregion

        public static LocalVariance Build(double[] maturityPillars, RrFunction[] pillarVariances)
        {
            RrFunction y = RrFunctions.Affine(1.0, 0.0);

            // (y * (dv/dy) / (2 * v) - 1)^2 + 1/2 * (d^2 v/dy^2) - 1/4 * (1/4 + 1/v) * (dv/dy)^2 
            RrFunction[] localVarianceDenominators = pillarVariances.Map(v =>
            {
                RrFunction deriv = v.Derivative();
                RrFunction secondDeriv = deriv.Derivative();

                RrFunction denom = y * deriv / (2.0 * v) - 1.0;
                denom = denom * denom;
                denom += 0.5 * secondDeriv;
                denom -= 0.25 * (0.25 + 1.0 / v) * deriv * deriv;
                return denom;
            });

            // dv/dt
            RrFunction[] varianceTimeDerivatives = EnumerableUtils.For(0, pillarVariances.Length, i =>
            {
                if (i == pillarVariances.Length - 1)
                    return pillarVariances[i] / (maturityPillars[i]);
                var step = maturityPillars[i + 1] - maturityPillars[i];
                return (pillarVariances[i + 1] - pillarVariances[i]) / step;
            });

            return new LocalVariance(maturityPillars, varianceTimeDerivatives, localVarianceDenominators);
        }
        
        public double Eval(double t, double y)
        {
            var stepIndex = maturityStepSearcher.LocateLeftIndex(t);

            if (stepIndex < 0)
                return 0.0;

            double localVariance = varianceTimeDerivatives[stepIndex].Eval(y);

            RrFunction leftDenom = localVarianceDenominators[stepIndex];
            if (stepIndex < maturityPillars.Length - 1)
            {
                var rightDenom = localVarianceDenominators[stepIndex + 1];
                double w = (t - maturityPillars[stepIndex]) / (maturityPillars[stepIndex + 1] - maturityPillars[stepIndex]);

                localVariance /= (1.0 - w) * leftDenom.Eval(y) + w * rightDenom.Eval(y);
            }
            else
            {
                localVariance /= t * leftDenom.Eval(y) / maturityPillars[stepIndex];
            }
            return localVariance;
        }

    }

}