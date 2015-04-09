using System;
using NUnit.Framework;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths;

namespace test.Maths
{
    [TestFixture]
    public class Cholesky_Test
    {
        #region private methods
        private static  void TestDecomposition(double[,] a, double precision)
        {
            var d = Cholesky.Decomposition(a);
            var test = d.Prod(d.Tranpose());

            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    var err = Math.Abs(a[i, j] - test[i, j]);
                    Assert.IsTrue(err < precision * DoubleUtils.Epsilon);
                }
            }
        }
        #endregion
        [Test]
        public void TestDecomposition1()
        {
            const double rho = 0.5;
            var a = new[,]
            {
                {1.0, rho, rho, rho},
                {rho, 1.0, rho, rho},
                {rho, rho, 1.0, rho},
                {rho, rho, rho, 1.0}
            };

            TestDecomposition(a, 2);
        }
        [Test]
        public void TestDecomposition2()
        {
            const double rho = 0.99;
            var a = new[,]
            {
                {1.0, rho, rho, rho},
                {rho, 1.0, rho, rho},
                {rho, rho, 1.0, rho},
                {rho, rho, rho, 1.0}
            };

            TestDecomposition(a, 2);
        }
        [Test]
        public void TestDecomposition3()
        {
            const double rho = 0.5;
            var a = new[,]
            {
                {1.0, rho, rho, rho},
                {rho, 1.0, 1.0, rho},
                {rho, 1.0, 1.0, rho},
                {rho, rho, rho, 1.0}
            };

            TestDecomposition(a, 2);
        }
        [Test]
        public void TestDecomposition4()
        {
            const double rho = 0.5;
            var a = new[,]
            {
                {1.0, 1.0, rho, rho},
                {1.0, 1.0, rho, rho},
                {rho, rho, 1.0, rho},
                {rho, rho, rho, 1.0}
            };

            TestDecomposition(a, 2);
        }
    }
}
