using System;
using NUnit.Framework;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths;

namespace test.Maths
{
    [TestFixture]
    public class CholeskyTest
    {
        [Test, TestCaseSource("Matrix")]
        public void New_TestDecomposition(double[,] a, double precision)
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

        private static object[] Matrix()
        {
            var a1 = new[,]
            {
                {1.0, 0.5, 0.5, 0.5},
                {0.5, 1.0, 0.5, 0.5},
                {0.5, 0.5, 1.0, 0.5},
                {0.5, 0.5, 0.5, 1.0}
            };
            var a2 = new[,]
            {
                {1.0, 0.99, 0.99, 0.99},
                {0.99, 1.0, 0.99, 0.99},
                {0.99, 0.99, 1.0, 0.99},
                {0.99, 0.99, 0.99, 1.0}
            };
            var a3 = new[,]
            {
                {1.0, 0.5, 0.5, 0.5},
                {0.5, 1.0, 1.0, 0.5},
                {0.5, 1.0, 1.0, 0.5},
                {0.5, 0.5, 0.5, 1.0}
            };

            var a4 = new[,]
            {
                {1.0, 1.0, 0.5, 0.5},
                {1.0, 1.0, 0.5, 0.5},
                {0.5, 0.5, 1.0, 0.5},
                {0.5, 0.5, 0.5, 1.0}
            };
            
            return  new object[]
            {
                new object[]{a1, 2.0},
                new object[]{a2, 2.0},
                new object[]{a3, 2.0},
                new object[]{a4, 2.0}
            };
        }
    }
}
