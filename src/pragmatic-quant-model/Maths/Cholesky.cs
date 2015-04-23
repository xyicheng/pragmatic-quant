using System;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.Maths
{
    public static class Cholesky
    {
        /// <summary>
        /// Return lower triangular matrix L s.t. A=L*transpose(L)
        /// </summary>
        /// <param name="a"></param>
        /// <param name="negativeTolerance"></param>
        /// <returns></returns>
        public static double[,] Decomposition(double[,] a, double negativeTolerance)
        {
            int n = a.GetLength(0);
            if (n != a.GetLength(1))
                throw new Exception("Cholesky : square matrix expected !");

            var el = a.Copy();
            for (int i = 0; i < n; i++)
            {
                for (int j = i; j < n; j++)
                {
                    double sum = el[i, j];
                    for (int k = i - 1; k >= 0; k--) sum -= el[i, k] * el[j, k];
                    if (i == j)
                    {
                        if (sum < -negativeTolerance * Math.Abs(el[i, i]))
                            throw new Exception("Cholesky failed !");
                        if (sum <= 0.0)
                        {
                            el[i, i] = 0.0;
                            break;
                        }
                        el[i, i] = Math.Sqrt(sum);
                    }
                    else
                    {
                        el[j, i] = sum / el[i, i];
                    }
                }
            }

            for (int i = 0; i < n; i++)
                for (int j = 0; j < i; j++)
                    el[j, i] = 0.0;

            return el;
        }
        /// <summary>
        /// Return lower triangular matrix L s.t. A=L*transpose(L)
        /// </summary>
        /// <param name="a"></param>
        /// <returns></returns>
        public static double[,] Decomposition(double[,] a)
        {
            return Decomposition(a, DoubleUtils.MachineEpsilon);
        }
    }
}
