using System;

namespace pragmatic_quant_model.Maths
{
    public static class MatrixUtils
    {
        /// <summary>
        /// Compute the matrix product : a * b, stored in result
        /// </summary>
        /// <param name="result"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void Mult(ref double[,] result, double[,] a, double[,] b)
        {
            if (result.GetLength(0) != a.GetLength(0))
                throw new Exception("MatrixUtils : result.GetLength(0) should be equal to a.GetLength(0)");
            if (result.GetLength(1) != b.GetLength(1))
                throw new Exception("MatrixUtils : result.GetLength(1) should be equal to b.GetLength(1)");
            if (a.GetLength(1) != b.GetLength(0))
                throw new Exception("MatrixUtils : a.GetLength(1) should be equal to b.GetLength(0)");

            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    result[i, j] = 0;
                    for (int k = 0; k < a.GetLength(1); k++)
                    {
                        result[i, j] += a[i, k] * b[k, j];
                    }
                }
            }
        }
        /// <summary>
        /// Compute the matrix transposition of a, stored in result
        /// </summary>
        /// <param name="result"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void Tranpose(ref double[,] result, double[,] a)
        {
            if (result.GetLength(0) != a.GetLength(1))
                throw new Exception("MatrixUtils : result.GetLength(0) should be equal to a.GetLength(1)");
            if (result.GetLength(1) != a.GetLength(0))
                throw new Exception("MatrixUtils : result.GetLength(1) should be equal to a.GetLength(0)");

            for (int i = 0; i < a.GetLength(0); i++)
                for (int j = 0; j < a.GetLength(1); j++)
                    result[j, i] = a[i, j];
        }

        public static double[,] Copy(this double[,] m)
        {
            var c = new double[m.GetLength(0), m.GetLength(1)];
            for (int i = 0; i < m.GetLength(0); i++)
                for (int j = 0; j < m.GetLength(1); j++)
                    c[i, j] = m[i, j];
            return c;
        }
        public static double[,] Mult(this double[,] m, double[,] a)
        {
            var result = new double[m.GetLength(0), a.GetLength(1)];
            Mult(ref result, m, a);
            return result;
        }
        public static double[,] Tranpose(this double[,] m)
        {
            var result = new double[m.GetLength(1), m.GetLength(0)];
            Tranpose(ref result, m);
            return result;
        }
    }
}