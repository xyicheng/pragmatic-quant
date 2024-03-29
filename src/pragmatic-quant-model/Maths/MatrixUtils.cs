﻿using System;
using System.Diagnostics.Contracts;

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
        /// Multiply matrix row with index rowIndex by  w
        /// </summary>
        public static void MultRow(ref double[,] result, int rowIndex, double w)
        {
            if (rowIndex < result.GetLowerBound(0) || rowIndex > result.GetUpperBound(0))
                throw new IndexOutOfRangeException();
            for (int j = 0; j < result.GetLength(1); j++)
            {
                result[rowIndex, j] *= w;
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

        public static void CorrelFromCovariance(ref double[,] covariance)
        {
            if (covariance.GetLength(0)!=covariance.GetLength(1))
                throw new Exception("Square matrix expected");
            int size = covariance.GetLength(0);
            for (int i = 0; i < size; i++)
            {
                var stdDev = Math.Sqrt(covariance[i, i]);
                for (int j = 0; j < size; j++)
                {
                    covariance[i, j] /= stdDev;
                    covariance[j, i] /= stdDev;
                }
            }
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
        public static double[,] Correlation(this double[,] covariance)
        {
            var correl = covariance.Copy();
            CorrelFromCovariance(ref correl);
            return correl;
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

        /// <summary>
        /// Compute transpose(left) * m * right
        /// </summary>
        /// <param name="m"></param>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns></returns>
        public static double BilinearProd(this double[,] m, double[] left, double[] right)
        {
            double prod = 0.0;
            for (int i = 0; i < m.GetLength(0); i++)
            {
                double partial = 0.0;
                for (int j = 0; j < m.GetLength(1); j++)
                {
                    partial += m[i, j] * right[j];
                }
                prod += left[i] * partial;
            }
            return prod;
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

        public static void Add(ref double[] a, double[] b)
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] += b[i];
            }
        }
        public static void AddW(ref double[] a, double[] b, double weight)
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] += b[i] * weight;
            }
        }
        public static void AddW(ref double[] a, double[] b, double[] weight)
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] += b[i] * weight[i];
            }
        }
        public static void Substract(ref double[] a, double[] b)
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] -= b[i];
            }
        }
        public static void Mult(ref double[] a, double[] b)
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] *= b[i];
            }
        }
        public static void Mult(ref double[] a, double w)
        {
            for (int i = 0; i < a.Length; i++)
            {
                a[i] *= w;
            }
        }
        
        public static double DotProduct(this double[] a, double[] b)
        {
            Contract.Requires(a.Length == b.Length);
            var dotProduct = 0.0;
            for (int i = 0; i < a.Length; i++)
            {
                dotProduct += a[i] * b[i];
            }
            return dotProduct;
        }
        public static double[] Add(this double[] a, double[] b)
        {
            var add = new double[a.Length];
            Array.Copy(a, add, a.Length);
            Add(ref add, b);
            return add;
        }
        public static double[] Add(this double[] a, double[] b, double weight)
        {
            var addW = new double[a.Length];
            Array.Copy(a, addW, a.Length);
            AddW(ref addW, b, weight);
            return addW;
        }
        public static double[] Substract(this double[] a, double[] b)
        {
            var sub = new double[a.Length];
            Array.Copy(a, sub, a.Length);
            Substract(ref sub, b);
            return sub;
        }
        public static double[] Mult(this double[] a, double[] b)
        {
            var mult = new double[a.Length];
            Array.Copy(a, mult, a.Length);
            Mult(ref mult, b);
            return mult;
        }
        public static double[] Mult(this double[] a, double weight)
        {
            var mult = new double[a.Length];
            Array.Copy(a, mult, a.Length);
            Mult(ref mult, weight);
            return mult;
        }
        public static double[] Copy(this double[] a)
        {
            var c = new double[a.Length];
            a.CopyTo(c, 0);
            return c;
        }
    }  
}