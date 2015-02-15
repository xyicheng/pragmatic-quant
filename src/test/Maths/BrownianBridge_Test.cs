using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Sobol;

namespace test.Maths
{
    [TestClass]
    public class BrownianBridge_Test
    {
        #region private method
        private void SinglePathVariance(double[] dates, int nbPaths, double precision, bool relativePrecision)
        {
            var brownian = BrownianBridge.Create(dates);

            var chrono = new Stopwatch();
            chrono.Start();
            
            var sobol = new SobolRsg(dates.Length, 0, SobolRsg.DirectionIntegers.JoeKuoD5);
            var variances = new double[dates.Length];
            for (int i = 0; i < nbPaths; ++i)
            {
                var uniforms = sobol.NextSequence();
                var gaussians = uniforms.Select(NormalDistribution.CumulativeInverse).ToArray();
                var path = brownian.NextPath(gaussians);
                for (int j = 0; j < variances.Length; ++j)
                    variances[j] += path[j] * path[j];
            }
            for (int j = 0; j < variances.Length; ++j)
                variances[j] /= nbPaths;

            chrono.Stop();
            Console.WriteLine("Elapsed " + chrono.Elapsed);
            UnitTestUtils.EqualDoubleArray(variances, dates, precision, relativePrecision);
        }
        #endregion
        [TestMethod]
        public void SinglePathVariance1()
        {
            var dates = new[] {0.0, 0.083333333333, 0.25, 0.5, 0.901, 2.0, 3.10184, 4.0, 5.0};
            SinglePathVariance(dates, 1000000, 2.5e-4, false);
        }
        [TestMethod]
        public void SinglePathVariance2()
        {
            var dates = GridUtils.RegularGrid(1.0, 10.0, 10);
            var rand = new Random(2584);
            dates = dates.Select(d => d + -0.45 + 0.9 * rand.NextDouble()).ToArray();
            SinglePathVariance(dates, 1000000, 8.5e-5, true);
        }

        #region private method
        private void MultiDimPathVariance(double[] dates, int brownianDim, int nbPaths, double precision)
        {
            var brownian = BrownianBridge.Create(dates);

            var chrono = new Stopwatch();
            chrono.Start();

            var sobol = new SobolRsg(dates.Length * brownianDim, 0, SobolRsg.DirectionIntegers.JoeKuoD5);
            var variances = new double[dates.Length][,];
            for (int j = 0; j < dates.Length; ++j) variances[j] = new double[brownianDim, brownianDim];

            for (int i = 0; i < nbPaths; ++i)
            {
                var uniforms = sobol.NextSequence();
                var gaussians = uniforms.Select(NormalDistribution.CumulativeInverse).ToArray();
                var path = brownian.NextPath(gaussians, brownianDim);
                for (int j = 0; j < variances.Length; ++j)
                {
                    VectorUtils.AddCov(ref variances[j], path[j], path[j]);
                }
            }
            for (int j = 0; j < variances.Length; ++j)
            {
                MatrixUtils.MultTo(ref variances[j], 1.0 / nbPaths);
            }

            chrono.Stop();
            Console.WriteLine("Elapsed " + chrono.Elapsed);

            for (int j = 0; j < variances.Length; ++j)
            {
                var covRef = MatrixUtils.Diagonal(brownianDim, dates[j]);
                UnitTestUtils.EqualDoubleMatrix(variances[j], covRef, precision);
            }
        }
        #endregion
        [TestMethod]
        public void MultiDimPathVariance()
        {
            var dates = new[] { 0.0, 0.083333333333, 0.25, 0.5, 0.901, 2.0, 3.10184, 4.0, 5.0 };
            MultiDimPathVariance(dates, 3, 1000000, 2.5e-4);
        }
    }
}
