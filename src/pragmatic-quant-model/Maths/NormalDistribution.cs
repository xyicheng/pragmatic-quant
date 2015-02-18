// ORIGINAL SOURCE CODE :
// source code resides at www.jaeckel.org/LetsBeRational.7z .
//
// ======================================================================================
// Copyright © 2013-2014 Peter Jäckel.
// 
// Permission to use, copy, modify, and distribute this software is freely granted,
// provided that this notice is preserved.
//
// WARRANTY DISCLAIMER
// The Software is provided "as is" without warranty of any kind, either express or implied,
// including without limitation any implied warranties of condition, uninterrupted use,
// merchantability, fitness for a particular purpose, or non-infringement.
// ======================================================================================
//

using System;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.Maths
{
    public static class NormalDistribution
    {
        #region Cumulative const
        private const double Cdf_Asymptotic_First_Threshold = -10.0;
        private static readonly double CdfAsymptoticSecondThreshold = -1 / Math.Sqrt(DoubleUtils.Epsilon);
        #endregion
        /// <summary>
        ///Cumulative normal distribution function
        ///For this implementation see M. Abramowitz and I. Stegun,
        ///Handbook of Mathematical Functions,
        ///Dover Publications, New York (1972)
        /// </summary>
        public static double Cumulative(double z)
        {
            if (z <= Cdf_Asymptotic_First_Threshold)
            {
                // Asymptotic expansion for very negative z following (26.2.12) on page 408
                // in M. Abramowitz and A. Stegun, Pocketbook of Mathematical Functions, ISBN 3-87144818-4.
                double sum = 1;
                if (z >= CdfAsymptoticSecondThreshold)
                {
                    double zsqr = z * z, i = 1, g = 1, a = double.MaxValue, lasta;
                    do
                    {
                        lasta = a;
                        double x = (4 * i - 3) / zsqr;
                        double y = x * ((4 * i - 1) / zsqr);
                        a = g * (x - y);
                        sum -= a;
                        g *= y;
                        ++i;
                        a = Math.Abs(a);
                    } while (lasta > a && a >= Math.Abs(sum * DoubleUtils.Epsilon));
                }
                return -Density(z) * sum / z;
            }
            return 0.5 * ErrorFunctions.Erfc(-z * MathConsts.InvSqrt2);
        }

        /// <summary>
        /// Density of normal distribution
        /// </summary>
        public static double Density(double z)
        {
            return MathConsts.InvSqrtTwoPi * Math.Exp(-0.5 * z * z);
        }

        /// <summary>
        /// Inverse cumulative normal distribution function
        ///
        /// ALGORITHM AS241  APPL. STATIST. (1988) VOL. 37, NO. 3
        ///
        /// Produces the normal deviate Z corresponding to a given lower
        /// tail area of p; Z is accurate to about 1 part in 10**16.
        /// see http://lib.stat.cmu.edu/apstat/241
        ///
        /// </summary>
        public static double CumulativeInverse(double p)
        {
            if (p <= 0)
                return Math.Log(p);
            if (p >= 1)
                return Math.Log(1 - p);

            double q = p - 0.5;
            if (Math.Abs(q) <= 0.425)
            {
                double r = 0.180625 - q * q;
                return q * (((((((2.5090809287301226727E+3 * r + 3.3430575583588128105E+4) * r + 6.7265770927008700853E+4) * r + 4.5921953931549871457E+4) * r + 1.3731693765509461125E+4) * r + 1.9715909503065514427E+3) * r + 1.3314166789178437745E+2) * r + 3.3871328727963666080E0)
                    / (((((((5.2264952788528545610E+3 * r + 2.8729085735721942674E+4) * r + 3.9307895800092710610E+4) * r + 2.1213794301586595867E+4) * r + 5.3941960214247511077E+3) * r + 6.8718700749205790830E+2) * r + 4.2313330701600911252E+1) * r + 1.0);
            }
            else
            {
                double r = q < 0.0 ? p : 1.0 - p;
                r = Math.Sqrt(-Math.Log(r));
                double ret;
                if (r < 5.0)
                {
                    r = r - 1.6;
                    ret = (((((((7.74545014278341407640E-4 * r + 2.27238449892691845833E-2) * r + 2.41780725177450611770E-1) * r + 1.27045825245236838258E0) * r + 3.64784832476320460504E0) * r + 5.76949722146069140550E0) * r + 4.63033784615654529590E0) * r + 1.42343711074968357734E0)
                        / (((((((1.05075007164441684324E-9 * r + 5.47593808499534494600E-4) * r + 1.51986665636164571966E-2) * r + 1.48103976427480074590E-1) * r + 6.89767334985100004550E-1) * r + 1.67638483018380384940E0) * r + 2.05319162663775882187E0) * r + 1.0);
                }
                else
                {
                    r = r - 5.0;
                    ret = (((((((2.01033439929228813265E-7 * r + 2.71155556874348757815E-5) * r + 1.24266094738807843860E-3) * r + 2.65321895265761230930E-2) * r + 2.96560571828504891230E-1) * r + 1.78482653991729133580E0) * r + 5.46378491116411436990E0) * r + 6.65790464350110377720E0)
                        / (((((((2.04426310338993978564E-15 * r + 1.42151175831644588870E-7) * r + 1.84631831751005468180E-5) * r + 7.86869131145613259100E-4) * r + 1.48753612908506148525E-2) * r + 1.36929880922735805310E-1) * r + 5.99832206555887937690E-1) * r + 1.0);
                }
                return q < 0.0 ? -ret : ret;
            }

        }

        /// <summary>
        /// Moro Inverse cumulative normal distribution
        /// Uses Beasly and Springer approximation, with an improved
        /// approximation for the tails. See Boris Moro,
        /// "The Full Monte", 1995, Risk Magazine.
        /// </summary>
        public static double FastCumulativeInverse(double p)
        {
            if (p <= 0)
                return Math.Log(p);
            if (p >= 1)
                return Math.Log(1 - p);

            double result;
            double q = p - 0.5;

            if (Math.Abs(q) < 0.42)
            {
                // Beasley and Springer, 1977
                result = q * q;
                result = q * (((-25.44106049637 * result + 41.39119773534) * result + -18.61500062529) * result + 2.50662823884) 
                    / ((((3.13082909833 * result + -21.06224101826) * result + 23.08336743743) * result + -8.47351093090) * result + 1.0);
            }
            else
            {
                // improved approximation for the tail (Moro 1995)
                if (p < 0.5)
                    result = p;
                else
                    result = 1.0 - p;
                result = Math.Log(-Math.Log(result));
                result = 0.3374754822726147 + result * (0.9761690190917186 + result * (0.1607979714918209 + result * (0.0276438810333863
                        + result * (0.0038405729373609 + result * (0.0003951896511919 + result * (0.0000321767881768 + result * (0.0000002888167364 + result * 0.0000003960315187)))))));
                if (p < 0.5)
                    result = -result;
            }

            return result;
        }
    }
}
