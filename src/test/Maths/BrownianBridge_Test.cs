using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Sobol;
using pragmatic_quant_model.Maths.Stochastic;

namespace test.Maths
{
    [TestFixture]
    public class BrownianBridge_Test
    {
        #region private method
        private void MultiDimPathCovariance(double[] dates, int brownianDim, int nbPaths, double precision)
        {
            var brownian = BrownianBridge.Create(dates);

            var chrono = new Stopwatch();
            chrono.Start();

            var gaussianGen = RandomGenerators.GaussianSobol(SobolDirection.JoeKuoD5)
                                              .Build(dates.Length * brownianDim);
            var covariances = new double[dates.Length][,];
            for (int j = 0; j < dates.Length; ++j) 
                covariances[j] = new double[brownianDim, brownianDim];

            for (int i = 0; i < nbPaths; ++i)
            {
                var gaussians = gaussianGen.Next();
                var path = brownian.Path(gaussians, brownianDim);
                for (int j = 0; j < covariances.Length; ++j)
                {
                    VectorUtils.AddCov(ref covariances[j], path[j], path[j]);
                }
            }
            for (int j = 0; j < covariances.Length; ++j)
            {
                MatrixUtils.MultTo(ref covariances[j], 1.0 / nbPaths);
            }

            chrono.Stop();
            Console.WriteLine("Elapsed " + chrono.Elapsed);

            for (int j = 0; j < covariances.Length; ++j)
            {
                var covRef = MatrixUtils.Diagonal(brownianDim, dates[j]);
                UnitTestUtils.EqualDoubleMatrix(covariances[j], covRef, precision);
            }
        }
        #endregion
        [Test]
        public void MultiDimPathCovariance1()
        {
            var dates = new[] { 0.0, 0.083333333333, 0.25, 0.5, 0.901, 2.0, 3.10184, 4.0, 5.0 };
            MultiDimPathCovariance(dates, 3, 1000000, 2.5e-4);
        }
        [Test]
        public void MultiDimPathCovariance2()
        {
            var dates = GridUtils.RegularGrid(1.0, 10.0, 100);
            MultiDimPathCovariance(dates, 3, 1000000, 6.0e-4);
        }


        [Test, TestCaseSource(typeof(BrownianBridgeTestDatas), "SinglePathCovariance")]
        public void SinglePathCovariance(double[] dates, double epsilon, double precision)
        {
            var brownian = BrownianBridge.Create(dates);
            var dim = brownian.GaussianSize(1);
            var jacobian = FiniteDifferenceUtils.CenteredJacobian(x => brownian.Path(x), dim, new double[dim], epsilon);
            var covariance = jacobian.Prod(jacobian.Tranpose());

            for(int i=0; i< dates.Length; i++)
                for (int j = 0; j < dates.Length; j++)
                {
                    var cov_ij = Math.Min(dates[i], dates[j]);
                    var error = Math.Abs(cov_ij - covariance[i, j]);
                    Assert.LessOrEqual(error, precision);
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
