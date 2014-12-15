using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths;

namespace test.Maths
{
    [TestClass]
    public class Cholesky_test
    {
        #region private methods
        private static  void TestDecomposition(double[,] a, double precision)
        {
            var d = Cholesky.Decomposition(a);
            var test = d.Mult(d.Tranpose());

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
        [TestMethod]
        public void TestDecomposition1()
        {
            var rho = 0.5;
            var a = new[,]
            {
                {1.0, rho, rho, rho},
                {rho, 1.0, rho, rho},
                {rho, rho, 1.0, rho},
                {rho, rho, rho, 1.0}
            };

            TestDecomposition(a, 2);
        }
        [TestMethod]
        public void TestDecomposition2()
        {
            var rho = 0.99;
            var a = new[,]
            {
                {1.0, rho, rho, rho},
                {rho, 1.0, rho, rho},
                {rho, rho, 1.0, rho},
                {rho, rho, rho, 1.0}
            };

            TestDecomposition(a, 2);
        }
        [TestMethod]
        public void TestDecomposition3()
        {
            var rho = 0.5;
            var a = new[,]
            {
                {1.0, rho, rho, rho},
                {rho, 1.0, 1.0, rho},
                {rho, 1.0, 1.0, rho},
                {rho, rho, rho, 1.0}
            };

            TestDecomposition(a, 2);
        }
        [TestMethod]
        public void TestDecomposition4()
        {
            var rho = 0.5;
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
