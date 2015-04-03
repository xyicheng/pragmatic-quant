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
        public static void Prod(ref double[,] result, double[,] a, double[,] b)
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
                    result[i, j] = 0.0;
                    for (int k = 0; k < a.GetLength(1); k++)
                    {
                        result[i, j] += a[i, k] * b[k, j];
                    }
                }
            }
        }
        /// <summary>
        /// Multiply matrix result by  w  : result = w * result
        /// </summary>
        public static void MultTo(ref double[,] result, double w)
        {
            for (int i = 0; i < result.GetLength(0); i++)
            {
                for (int j = 0; j < result.GetLength(1); j++)
                {
                    result[i, j] *= w;
                }
            }
        }
        /// <summary>
        /// Compute the matrix transposition of a, stored in result
        /// </summary>
        /// <param name="result"></param>
        /// <param name="a"></param>
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
        public static double[,] Prod(this double[,] m, double[,] a)
        {
            var result = new double[m.GetLength(0), a.GetLength(1)];
            Prod(ref result, m, a);
            return result;
        }
        public static double[,] Mult(this double[,] m, double w)
        {
            var result = Copy(m);
            MultTo(ref result, w);
            return result;
        }
        public static double[,] Tranpose(this double[,] m)
        {
            var result = new double[m.GetLength(1), m.GetLength(0)];
            Tranpose(ref result, m);
            return result;
        }

        public static double[,] Id(int size)
        {
            return Diagonal(size, 1.0);
        }
        public static double[,] Diagonal(int size, double a)
        {
            var result = new double[size, size];
            for (int i = 0; i < size; i++)
                result[i, i] = a;
            return result;
        }
    }

    public static class VectorUtils
    {
        /// <summary>
        /// Add transp(a) * b to result
        /// </summary>
        /// <param name="result"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void AddCov(ref double[,] result, double[] a, double[] b)
        {
            if (a.Length != b.Length)
                throw new Exception("MatrixUtils : a.Length should be equal to b.Length");
            if (result.GetLength(0) != a.Length)
                throw new Exception("MatrixUtils : result.GetLength(0) should be equal to a.Length");
            if (result.GetLength(1) != a.Length)
                throw new Exception("MatrixUtils : result.GetLength(1) should be equal to a.Length");

            for (int i = 0; i < a.Length; i++)
            {
                for (int j = 0; j < a.Length; j++)
                {
                    result[i, j] += a[i] * b[j];
                }
            }
        }
    }
}