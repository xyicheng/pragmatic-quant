using System;
using System.Collections;
using System.Linq;
using NUnit.Framework;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Stochastic;

namespace test.Maths
{
    [TestFixture]
    public class BrownianBridge_Test
    {
        #region private methods
        private double[] BrownianPath1d(BrownianBridge bbridge,double[] x)
        {
            return bbridge.Path(x, 1).Map(v => v[0]);
        }
        #endregion
        [Test, TestCaseSource(typeof(BrownianBridgeTestDatas), "SinglePathCovariance")]
        public void SinglePathCovariance(double[] dates, double epsilon, double precision)
        {
            var brownian = BrownianBridge.Create(dates);
            var dim = brownian.GaussianSize(1);
            var jacobian = FiniteDifferenceUtils.CenteredJacobian(x => BrownianPath1d(brownian, x), dim, new double[dim], epsilon);
            var covariance = jacobian.Prod(jacobian.Tranpose());

            for(int i=0; i< dates.Length; i++)
                for (int j = 0; j < dates.Length; j++)
                {
                    var cov_ij = Math.Min(dates[i], dates[j]);
                    var error = Math.Abs(cov_ij - covariance[i, j]);
                    Assert.LessOrEqual(error, precision);
                }
        }
        
        #region private methods
        private static double[] PathInc1D(BrownianBridge bridge, double[] x)
        {
            return bridge.PathIncrements(x, 1).Map(v => v[0]);
        }
        #endregion
        [Test, TestCaseSource(typeof (BrownianBridgeTestDatas), "SinglePathCovariance")]
        public void SinglePathIncrementCovariance(double[] dates, double epsilon, double precision)
        {
            var brownian = BrownianBridge.Create(dates);
            var dim = brownian.GaussianSize(1);
            var jacobian = FiniteDifferenceUtils.CenteredJacobian(x => PathInc1D(brownian, x), dim, new double[dim], epsilon);
            var covariance = jacobian.Prod(jacobian.Tranpose());
            
            for (int i = 0; i < dates.Length; i++)
            {
                var error = Math.Abs(covariance[i, i] - (i == 0 ? dates[0] : dates[i] - dates[i - 1]));
                Assert.LessOrEqual(error, precision);

                for (int j = 0; j < i; j++)
                {
                    var errorij = Math.Abs(covariance[i, j]);
                    Assert.LessOrEqual(errorij, precision);
                    Assert.IsTrue(DoubleUtils.MachineEquality(covariance[i, j], covariance[j, i]));
                }
            }
        }
    }

    public class BrownianBridgeTestDatas
    {
        public IEnumerable SinglePathCovariance
        {
            get
            {
                yield return new object[] { new[] { 0.0, 0.083333333333, 0.25, 0.5, 0.901, 2.0, 3.10184, 4.0, 5.0 }, 1.0e-8, 5.0e-15 };

                var dates = GridUtils.RegularGrid(1.0, 10.0, 10);
                var rand = new Random(2584);
                dates = dates.Select(d => d + -0.45 + 0.9 * rand.NextDouble()).ToArray();
                yield return new object[] {dates, 1.0e-8, 5.0e-15};
            }
        }
    }
}
