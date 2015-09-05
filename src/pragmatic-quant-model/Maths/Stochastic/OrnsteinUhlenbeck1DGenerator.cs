using System;
using System.Diagnostics;

namespace pragmatic_quant_model.Maths.Stochastic
{
    public class OrnsteinUhlenbeck1DGenerator : IProcessPathGenerator
    {
        #region private fields
        private readonly double[] dates;
        private readonly double[] stepSlopes;
        private readonly double[] stepDrifts;
        private readonly double[] stepVols;
        private readonly double initialValue;
        #endregion
        public OrnsteinUhlenbeck1DGenerator(double[] dates, double initialValue, double[] stepSlopes, double[] stepDrifts, double[] stepVols)
        {
            this.dates = dates;
            this.initialValue = initialValue;
            this.stepSlopes = stepSlopes;
            this.stepDrifts = stepDrifts;
            this.stepVols = stepVols;
            AllSimulatedDates = dates;
        }

        public IProcessPath Path(double[][] dWs)
        {
            var processPath = new double[dWs.Length][];

            var currentProcess = initialValue;
            for (int i = 0; i < dWs.Length; i++)
            {
                currentProcess = stepSlopes[i] * currentProcess + stepDrifts[i] + stepVols[i] * dWs[i][0];
                processPath[i] = new[] { currentProcess };
            }

            return new ProcessPath(dates, 1, processPath);
        }
        public double[] Dates { get { return dates; } }
        public int ProcessDim { get { return 1; } }
        public double[] AllSimulatedDates { get; private set; }
    }

    public class OrnsteinUhlenbeck1DGeneratorFactory
    {
        #region private methods
        private static double StepDrift(double start, double end, OrnsteinUhlenbeck ouProcess)
        {
            return OrnsteinUhlenbeckUtils.IntegratedDrift(ouProcess.Drift, ouProcess.MeanReversion, start).Eval(end);
        }
        private static double StepVol(double start, double end, OrnsteinUhlenbeck ouProcess)
        {
            var step = end - start;
            Debug.Assert(step >= 0.0);
            var stepIntegralVar = OrnsteinUhlenbeckUtils.IntegratedVariance(ouProcess.Volatility, ouProcess.MeanReversion, start)
                                                        .Eval(end);
            return step > 0.0 ? Math.Sqrt(stepIntegralVar / step) : 0.0;
        }
        #endregion
        public static OrnsteinUhlenbeck1DGenerator Build(double[] dates, OrnsteinUhlenbeck ouProcess)
        {
            var stepSlopes = new double[dates.Length];
            var stepDrifts = new double[dates.Length];
            var stepVols = new double[dates.Length];
            for (int i = 0; i < dates.Length; i++)
            {
                var previous = (i == 0) ? 0.0 : dates[i - 1];
                var step = dates[i] - previous;
                stepSlopes[i] = Math.Exp(-ouProcess.MeanReversion * step);
                stepDrifts[i] = StepDrift(previous, dates[i], ouProcess);
                stepVols[i] = StepVol(previous, dates[i], ouProcess);
            }

            return new OrnsteinUhlenbeck1DGenerator(dates, ouProcess.Value0, stepSlopes, stepDrifts, stepVols);
        }
    }
}