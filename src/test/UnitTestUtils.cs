using System;
using NUnit.Framework;

namespace test
{
    public class UnitTestUtils
    {
        public static void EqualMatrix<T>(T[,] a, T[,] b, Func<T, T, bool> equality)
        {
            Assert.AreEqual(a.GetLength(0), b.GetLength(0));
            Assert.AreEqual(a.GetLength(1), b.GetLength(1));
            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    Assert.IsTrue(equality(a[i, j], b[i, j]));
                }
            }
        }
        public static void EqualMatrix<T>(T[,] a, T[,] b)
        {
            EqualMatrix(a, b, (t1, t2) => t1.Equals(t2));
        }
        public static void EqualDoubleMatrix(double[,] a, double[,] b, double tolerance, bool relativeTolerance = false)
        {
            if (relativeTolerance) EqualMatrix(a, b, (u, v) => (Math.Abs(u / v - 1.0) < tolerance));
            else EqualMatrix(a, b, (u, v) => (Math.Abs(u - v) < tolerance));
        }

        public static void EqualArrays<T>(T[] a, T[] b, Func<T, T, bool> equality)
        {
            Assert.AreEqual(a.Length, b.Length);
            for (int i = 0; i < a.Length; ++i)
            {
                Assert.IsTrue(equality(a[i], b[i]));
            }
        }
        public static void EqualArrays<T>(T[] a, T[] b)
        {
            EqualArrays(a, b, (t1, t2) => t1.Equals(t2));
        }
        public static void EqualDoubleArray(double[] a, double[] b, double tolerance, bool relativeTolerance = false)
        {
            if (relativeTolerance) EqualArrays(a, b, (u, v) => (Math.Abs(u / v - 1.0) < tolerance));
            else EqualArrays(a, b, (u, v) => (Math.Abs(u - v) < tolerance));
        }
    }
}
