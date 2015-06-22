using System;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.Maths
{
    public static class FiniteDifferenceUtils
    {
        public static double[] CenteredGradient(Func<double[], double> f, double[] x, double epsilon)
        {
            var shifted_x = x.Copy();
            var gradient = new double[x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                var x_i = x[i];

                shifted_x[i] = x_i + epsilon;
                var f_xPlus = f(shifted_x);

                shifted_x[i] = x_i - epsilon;
                var f_xMinus = f(shifted_x);

                gradient[i] = (f_xPlus - f_xMinus) / (2.0 * epsilon);
                shifted_x[i] = x_i;
            }

            return gradient;
        }
        public static double[,] CenteredJacobian(Func<double[], double[]> f, int outDim, double[] x, double epsilon)
        {
            var shifted_x = x.Copy();
            var jacobian = new double[outDim, x.Length];
            for (int i = 0; i < x.Length; i++)
            {
                var x_i = x[i];

                shifted_x[i] = x_i + epsilon;
                var f_xPlus = f(shifted_x);

                shifted_x[i] = x_i - epsilon;
                var f_xMinus = f(shifted_x);

                var df_i = f_xPlus.Substract(f_xMinus);
                VectorUtils.Mult(ref df_i, 1.0 / (2.0 * epsilon));
                ArrayUtils.SetCol(ref jacobian, i, df_i);

                shifted_x[i] = x_i;
            }

            return jacobian;
        }
    }
}
