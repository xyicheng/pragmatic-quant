using System;
using NUnit.Framework;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths;

namespace test.Maths
{
    [TestFixture]
    public class NormalDistribution_Test
    {
        [Test]
        public void SpecialValues()
        {
            Assert.AreEqual(NormalDistribution.Cumulative(0.0), 0.5);
            Assert.AreEqual(NormalDistribution.Cumulative(double.PositiveInfinity), 1.0);
            Assert.AreEqual(NormalDistribution.Cumulative(double.NegativeInfinity), 0.0);
            Assert.AreEqual(NormalDistribution.Cumulative(double.MaxValue), 1.0);
            Assert.AreEqual(NormalDistribution.Cumulative(double.MinValue), 0.0);
        }

        [Test]
        public void CheckSymmetry()
        {
            var rand = new Random(1468);
            for (int i = 0; i < 10000; i++)
            {
                var x = rand.NextDouble() * 30.0; // random in [0, 30] 
                var norm_x = NormalDistribution.Cumulative(x);
                var norm_minusx = NormalDistribution.Cumulative(-x);
                var err = Math.Abs((norm_x + norm_minusx) - 1.0);
                Assert.IsTrue(DoubleUtils.EqualZero(err));
            }
        }

        #region private method
        private static double CdfProxy(double x)
        {//c.f. D.Lamberton, B.Lapeyre "Introduction au calcul stochastique applique a la finance" p158.
            if (x < 0.0) throw new Exception("positive value expected !");
                
            double t = 1 / (1 + 0.2316419 * x);
            return 1.0 - NormalDistribution.Density(x)
                   * t * (0.319381530 + t * (-0.356563782 + t * (1.781477937 + t * (-1.821255978 + t * 1.330274429))));
        }
        #endregion
        [Test]
        public void ProxyCrossCheck()
        {
            var rand = new Random(1515);
            for (int i = 0; i < 10000; i++)
            {
                var x = rand.NextDouble() * 5.0; // random in [0, 5] 
                var norm_x = NormalDistribution.Cumulative(x);
                var proxy_norm_x = CdfProxy(x);
                var err = Math.Abs((proxy_norm_x - norm_x) / norm_x);

                Assert.IsTrue(err < 1.4e-7);
            }
        }

        [Test]
        public void InverseLowerRegion()
        {
            var grid = GridUtils.RegularGrid(-37.0, -2.0, 1000);
            foreach (var x in grid)
            {
                var p = NormalDistribution.Cumulative(x);
                var proxy_x = NormalDistribution.CumulativeInverse(p); 
                var errRelative = Math.Abs((proxy_x - x) / x);
                Assert.IsTrue(errRelative < 6 * DoubleUtils.MachineEpsilon);
            }
        }

        [Test]
        public void InverseCentralRegion()
        {
            var grid = GridUtils.RegularGrid(-2.0, 2.0, 1000);
            foreach (var x in grid)
            {
                var p = NormalDistribution.Cumulative(x);
                var proxy_x = NormalDistribution.CumulativeInverse(p);
                var errRelative = Math.Abs(proxy_x - x);
                Assert.IsTrue(errRelative < 13 * DoubleUtils.MachineEpsilon);
            }
        }

        [Test]
        public void InverseHigherRegion()
        {
            var grid = GridUtils.RegularGrid(2.0, 6.0, 1000);
            foreach (var x in grid)
            {
                var p = NormalDistribution.Cumulative(x);
                var proxy_x = NormalDistribution.CumulativeInverse(p);
                var err = Math.Abs((proxy_x - x) / x);
                Assert.IsTrue(err < 1.0e-8);
            }
        }

        [Test]
        public void FastVsPreciseInverse()
        {
            var bound = NormalDistribution.CumulativeInverse(1.0e-6);
            var grid = GridUtils.RegularGrid(bound, -bound, 1000);
            foreach (var x in grid)
            {
                var p = NormalDistribution.Cumulative(x);
                var precise = NormalDistribution.CumulativeInverse(p);
                var fast = NormalDistribution.FastCumulativeInverse(p);
                var errRelative = Math.Abs(fast - precise);
                Assert.IsTrue(errRelative < 3.0e-9);
            }

        }
    }
}
