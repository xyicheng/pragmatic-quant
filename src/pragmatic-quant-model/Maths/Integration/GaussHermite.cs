using System;

namespace pragmatic_quant_model.Maths.Integration
{
    public class GaussHermite
    {
        #region private fields
        private const double Eps = 1.0e-14;
        private const double Pim4 = 0.7511255444649425;
        private const int MaxIt = 10;
        #endregion
        
        public static void GetQuadrature(int n, out double[] x, out double[] w)
        {
            x = new double[n];
            w = new double[n];

            int m = (n + 1) / 2;
            double z = double.NaN, pp = double.NaN;
            for (int i = 0; i < m; i++)
            {
                if (i == 0)
                {
                    z = Math.Sqrt(2.0 * n + 1.0) - 1.85575 * Math.Pow(2.0 * n + 1.0, -0.16667);
                }
                else if (i == 1)
                {
                    z -= 1.14 * Math.Pow(n, 0.426) / z;
                }
                else if (i == 2)
                {
                    z = 1.86 * z - 0.86 * x[0];
                }
                else if (i == 3)
                {
                    z = 1.91 * z - 0.91 * x[1];
                }
                else
                {
                    z = 2.0 * z - x[i - 2];
                }

                int its;
                for (its = 0; its < MaxIt; its++)
                {
                    double p1 = Pim4;
                    double p2 = 0.0;
                    for (int j = 0; j < n; j++)
                    {
                        double p3 = p2;
                        p2 = p1;
                        p1 = z * Math.Sqrt(2.0 / (j + 1.0)) * p2 - Math.Sqrt(j / (j + 1.0)) * p3;
                    }
                    pp = Math.Sqrt(2.0 * n) * p2;
                    double z1 = z;
                    z = z1 - p1 / pp;
                    if (Math.Abs(z - z1) <= Eps) break;
                }
                if (its >= MaxIt) 
                    throw new Exception("Too many iterations in Gauss Hermite");

                x[i] = z;
                x[n - 1 - i] = -z;
                w[i] = 2.0 / (pp * pp);
                w[n - 1 - i] = w[i];
            }

            VectorUtils.Mult(ref x, Math.Sqrt(2.0));
            VectorUtils.Mult(ref w, 1.0 / Math.Sqrt(Math.PI));
        } 
    }
}
