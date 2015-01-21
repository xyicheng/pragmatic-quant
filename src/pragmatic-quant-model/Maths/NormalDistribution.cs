// ORIGINAL SOURCE CODE
// This source code resides at www.jaeckel.org/LetsBeRational.7z .
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

        #region Cumulative inverse const
        private const double Split1 = 0.425;
        private const double Split2 = 5.0;
        private const double Const1 = 0.180625;
        private const double Const2 = 1.6;

        // Coefficients for P close to 0.5
        private const double A0 = 3.3871328727963666080E0;
        private const double A1 = 1.3314166789178437745E+2;
        private const double A2 = 1.9715909503065514427E+3;
        private const double A3 = 1.3731693765509461125E+4;
        private const double A4 = 4.5921953931549871457E+4;
        private const double A5 = 6.7265770927008700853E+4;
        private const double A6 = 3.3430575583588128105E+4;
        private const double A7 = 2.5090809287301226727E+3;
        private const double B1 = 4.2313330701600911252E+1;
        private const double B2 = 6.8718700749205790830E+2;
        private const double B3 = 5.3941960214247511077E+3;
        private const double B4 = 2.1213794301586595867E+4;
        private const double B5 = 3.9307895800092710610E+4;
        private const double B6 = 2.8729085735721942674E+4;
        private const double B7 = 5.2264952788528545610E+3;
        // Coefficients for P not close to 0, 0.5 or 1.
        private const double C0 = 1.42343711074968357734E0;
        private const double C1 = 4.63033784615654529590E0;
        private const double C2 = 5.76949722146069140550E0;
        private const double C3 = 3.64784832476320460504E0;
        private const double C4 = 1.27045825245236838258E0;
        private const double C5 = 2.41780725177450611770E-1;
        private const double C6 = 2.27238449892691845833E-2;
        private const double C7 = 7.74545014278341407640E-4;
        private const double D1 = 2.05319162663775882187E0;
        private const double D2 = 1.67638483018380384940E0;
        private const double D3 = 6.89767334985100004550E-1;
        private const double D4 = 1.48103976427480074590E-1;
        private const double D5 = 1.51986665636164571966E-2;
        private const double D6 = 5.47593808499534494600E-4;
        private const double D7 = 1.05075007164441684324E-9;
        // Coefficients for P very close to 0 or 1
        private const double E0 = 6.65790464350110377720E0;
        private const double E1 = 5.46378491116411436990E0;
        private const double E2 = 1.78482653991729133580E0;
        private const double E3 = 2.96560571828504891230E-1;
        private const double E4 = 2.65321895265761230930E-2;
        private const double E5 = 1.24266094738807843860E-3;
        private const double E6 = 2.71155556874348757815E-5;
        private const double E7 = 2.01033439929228813265E-7;
        private const double F1 = 5.99832206555887937690E-1;
        private const double F2 = 1.36929880922735805310E-1;
        private const double F3 = 1.48753612908506148525E-2;
        private const double F4 = 7.86869131145613259100E-4;
        private const double F5 = 1.84631831751005468180E-5;
        private const double F6 = 1.42151175831644588870E-7;
        private const double F7 = 2.04426310338993978564E-15;
        #endregion
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
            if (Math.Abs(q) <= Split1)
            {
                double r = Const1 - q * q;
                return q * (((((((A7 * r + A6) * r + A5) * r + A4) * r + A3) * r + A2) * r + A1) * r + A0) /
                   (((((((B7 * r + B6) * r + B5) * r + B4) * r + B3) * r + B2) * r + B1) * r + 1.0);
            }
            else
            {
                double r = q < 0.0 ? p : 1.0 - p;
                r = Math.Sqrt(-Math.Log(r));
                double ret;
                if (r < Split2)
                {
                    r = r - Const2;
                    ret = (((((((C7 * r + C6) * r + C5) * r + C4) * r + C3) * r + C2) * r + C1) * r + C0) /
                       (((((((D7 * r + D6) * r + D5) * r + D4) * r + D3) * r + D2) * r + D1) * r + 1.0);
                }
                else
                {
                    r = r - Split2;
                    ret = (((((((E7 * r + E6) * r + E5) * r + E4) * r + E3) * r + E2) * r + E1) * r + E0) /
                       (((((((F7 * r + F6) * r + F5) * r + F4) * r + F3) * r + F2) * r + F1) * r + 1.0);
                }
                return q < 0.0 ? -ret : ret;
            }
            

            
        }
    }
}
