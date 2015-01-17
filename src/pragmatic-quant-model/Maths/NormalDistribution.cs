using System;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.Maths
{
    public static class NormalDistribution
    {
        #region Sun Microsystems erf
        /*
        * ====================================================
        * Copyright (C) 1993 by Sun Microsystems, Inc. All rights reserved.
        *
        * Developed at SunPro, a Sun Microsystems, Inc. business.
        * Permission to use, copy, modify, and distribute this
        * software is freely granted, provided that this notice 
        * is preserved.
        * ====================================================
        */
        /* double erf(double x)
        * double erfc(double x)
        *                           x
        *                    2      |\
        *     erf(x)  =  ---------  | exp(-t*t)dt
        *                 sqrt(pi) \| 
        *                           0
        *
        *     erfc(x) =  1-erf(x)
        *  Note that 
        *              erf(-x) = -erf(x)
        *              erfc(-x) = 2 - erfc(x)
        *
        * Method:
        *      1. For |x| in [0, 0.84375]
        *          erf(x)  = x + x*R(x^2)
        *          erfc(x) = 1 - erf(x)           if x in [-.84375,0.25]
        *                  = 0.5 + ((0.5-x)-x*R)  if x in [0.25,0.84375]
        *         where R = P/Q where P is an odd poly of degree 8 and
        *         Q is an odd poly of degree 10.
        *                                               -57.90
        *                      | R - (erf(x)-x)/x | <= 2
        *      
        *
        *         Remark. The formula is derived by noting
        *          erf(x) = (2/sqrt(pi))*(x - x^3/3 + x^5/10 - x^7/42 + ....)
        *         and that
        *          2/sqrt(pi) = 1.128379167095512573896158903121545171688
        *         is close to one. The interval is chosen because the fix
        *         point of erf(x) is near 0.6174 (i.e., erf(x)=x when x is
        *         near 0.6174), and by some experiment, 0.84375 is chosen to
        *         guarantee the error is less than one ulp for erf.
        *
        *      2. For |x| in [0.84375,1.25], let s = |x| - 1, and
        *         c = 0.84506291151 rounded to single (24 bits)
        *              erf(x)  = sign(x) * (c  + P1(s)/Q1(s))
        *              erfc(x) = (1-c)  - P1(s)/Q1(s) if x > 0
        *                        1+(c+P1(s)/Q1(s))    if x < 0
        *              |P1/Q1 - (erf(|x|)-c)| <= 2**-59.06
        *         Remark: here we use the taylor series expansion at x=1.
        *              erf(1+s) = erf(1) + s*Poly(s)
        *                       = 0.845.. + P1(s)/Q1(s)
        *         That is, we use rational approximation to approximate
        *                      erf(1+s) - (c = (single)0.84506291151)
        *         Note that |P1/Q1|< 0.078 for x in [0.84375,1.25]
        *         where 
        *              P1(s) = degree 6 poly in s
        *              Q1(s) = degree 6 poly in s
        *
        *      3. For x in [1.25,1/0.35(~2.857143)], 
        *              erfc(x) = (1/x)*exp(-x*x-0.5625+R1/S1)
        *              erf(x)  = 1 - erfc(x)
        *         where 
        *              R1(z) = degree 7 poly in z, (z=1/x^2)
        *              S1(z) = degree 8 poly in z
        *
        *      4. For x in [1/0.35,28]
        *              erfc(x) = (1/x)*exp(-x*x-0.5625+R2/S2) if x > 0
        *                      = 2.0 - (1/x)*exp(-x*x-0.5625+R2/S2) if -6<x<0
        *                      = 2.0 - tiny            (if x <= -6)
        *              erf(x)  = sign(x)*(1.0 - erfc(x)) if x < 6, else
        *              erf(x)  = sign(x)*(1.0 - tiny)
        *         where
        *              R2(z) = degree 6 poly in z, (z=1/x^2)
        *              S2(z) = degree 7 poly in z
        *
        *      Note1:
        *         To compute exp(-x*x-0.5625+R/S), let s be a single
        *         precision number and s := x; then
        *              -x*x = -s*s + (s-x)*(s+x)
        *              exp(-x*x-0.5626+R/S) = 
        *                      exp(-s*s-0.5625)*exp((s-x)*(s+x)+R/S);
        *      Note2:
        *         Here 4 and 5 make use of the asymptotic series
        *                        exp(-x*x)
        *              erfc(x) ~ ---------- * ( 1 + Poly(1/x^2) )
        *                        x*sqrt(pi)
        *         We use rational approximation to approximate
        *              g(s)=f(1/x^2) = log(erfc(x)*x) - x*x + 0.5625
        *         Here is the error bound for R1/S1 and R2/S2
        *              |R1/S1 - f(x)|  < 2**(-62.57)
        *              |R2/S2 - f(x)|  < 2**(-61.52)
        *
        *      5. For inf > x >= 28
        *              erf(x)  = sign(x) *(1 - tiny)  (raise inexact)
        *              erfc(x) = tiny*tiny (raise underflow) if x > 0
        *                      = 2 - tiny if x<0
        *
        *      7. Special case:
        *              erf(0)  = 0, erf(inf)  = 1, erf(-inf) = -1,
        *              erfc(0) = 1, erfc(inf) = 0, erfc(-inf) = 2, 
        *              erfc/erf(NaN) is NaN
        */
        #region const fields
        private static readonly double Tiny = DoubleUtils.Epsilon;

        private const double
            One = 1.00000000000000000000e+00,
            Erx = 8.45062911510467529297e-01,
            //
            // Coefficients for approximation to  erf on [0,0.84375]
            //
            Efx = 1.28379167095512586316e-01,
            Efx8 = 1.02703333676410069053e+00,
            Pp0 = 1.28379167095512558561e-01,
            Pp1 = -3.25042107247001499370e-01,
            Pp2 = -2.84817495755985104766e-02,
            Pp3 = -5.77027029648944159157e-03,
            Pp4 = -2.37630166566501626084e-05,
            Qq1 = 3.97917223959155352819e-01,
            Qq2 = 6.50222499887672944485e-02,
            Qq3 = 5.08130628187576562776e-03,
            Qq4 = 1.32494738004321644526e-04,
            Qq5 = -3.96022827877536812320e-06,
            //
            // Coefficients for approximation to  erf  in [0.84375,1.25]
            //
            Pa0 = -2.36211856075265944077e-03,
            Pa1 = 4.14856118683748331666e-01,
            Pa2 = -3.72207876035701323847e-01,
            Pa3 = 3.18346619901161753674e-01,
            Pa4 = -1.10894694282396677476e-01,
            Pa5 = 3.54783043256182359371e-02,
            Pa6 = -2.16637559486879084300e-03,
            Qa1 = 1.06420880400844228286e-01,
            Qa2 = 5.40397917702171048937e-01,
            Qa3 = 7.18286544141962662868e-02,
            Qa4 = 1.26171219808761642112e-01,
            Qa5 = 1.36370839120290507362e-02,
            Qa6 = 1.19844998467991074170e-02,
            //
            // Coefficients for approximation to  erfc in [1.25,1/0.35]
            //
            Ra0 = -9.86494403484714822705e-03,
            Ra1 = -6.93858572707181764372e-01,
            Ra2 = -1.05586262253232909814e+01,
            Ra3 = -6.23753324503260060396e+01,
            Ra4 = -1.62396669462573470355e+02,
            Ra5 = -1.84605092906711035994e+02,
            Ra6 = -8.12874355063065934246e+01,
            Ra7 = -9.81432934416914548592e+00,
            Sa1 = 1.96512716674392571292e+01,
            Sa2 = 1.37657754143519042600e+02,
            Sa3 = 4.34565877475229228821e+02,
            Sa4 = 6.45387271733267880336e+02,
            Sa5 = 4.29008140027567833386e+02,
            Sa6 = 1.08635005541779435134e+02,
            Sa7 = 6.57024977031928170135e+00,
            Sa8 = -6.04244152148580987438e-02,
            //
            // Coefficients for approximation to  erfc in [1/.35,28]
            //
            Rb0 = -9.86494292470009928597e-03,
            Rb1 = -7.99283237680523006574e-01,
            Rb2 = -1.77579549177547519889e+01,
            Rb3 = -1.60636384855821916062e+02,
            Rb4 = -6.37566443368389627722e+02,
            Rb5 = -1.02509513161107724954e+03,
            Rb6 = -4.83519191608651397019e+02,
            Sb1 = 3.03380607434824582924e+01,
            Sb2 = 3.25792512996573918826e+02,
            Sb3 = 1.53672958608443695994e+03,
            Sb4 = 3.19985821950859553908e+03,
            Sb5 = 2.55305040643316442583e+03,
            Sb6 = 4.74528541206955367215e+02,
            Sb7 = -2.24409524465858183362e+01;
        #endregion
        private static double Erf(double x)
        {
            double R;
            double S;
            double s;
            double r;

            double ax = Math.Abs(x);

            if (ax < 0.84375)
            {
                /* |x|<0.84375 */
                if (ax < 3.7252902984e-09)
                {
                    /* |x|<2**-28 */
                    if (ax < Double.MinValue * 16)
                        return 0.125 * (8.0 * x + Efx8 * x); /*avoid underflow */
                    return x + Efx * x;
                }
                double z = x * x;
                r = Pp0 + z * (Pp1 + z * (Pp2 + z * (Pp3 + z * Pp4)));
                s = One + z * (Qq1 + z * (Qq2 + z * (Qq3 + z * (Qq4 + z * Qq5))));
                double y = r / s;
                return x + x * y;
            }
            if (ax < 1.25)
            {
                /* 0.84375 <= |x| < 1.25 */
                s = ax - One;
                double p = Pa0 + s * (Pa1 + s * (Pa2 + s * (Pa3 + s * (Pa4 + s * (Pa5 + s * Pa6)))));
                double q = One + s * (Qa1 + s * (Qa2 + s * (Qa3 + s * (Qa4 + s * (Qa5 + s * Qa6)))));
                if (x >= 0)
                    return Erx + p / q;
                return -Erx - p / q;
            }
            if (ax >= 6)
            {
/* inf>|x|>=6 */
                if (x >= 0)
                    return One - Tiny;
                return Tiny - One;
            }

            /* Starts to lose accuracy when ax~5 */
            s = One / (ax * ax);

            if (ax < 2.85714285714285)
            {
                /* |x| < 1/0.35 */
                R = Ra0 + s * (Ra1 + s * (Ra2 + s * (Ra3 + s * (Ra4 + s * (Ra5 + s * (Ra6 + s * Ra7))))));
                S = One + s * (Sa1 + s * (Sa2 + s * (Sa3 + s * (Sa4 + s * (Sa5 + s * (Sa6 + s * (Sa7 + s * Sa8)))))));
            }
            else
            {
                /* |x| >= 1/0.35 */
                R = Rb0 + s * (Rb1 + s * (Rb2 + s * (Rb3 + s * (Rb4 + s * (Rb5 + s * Rb6)))));
                S = One + s * (Sb1 + s * (Sb2 + s * (Sb3 + s * (Sb4 + s * (Sb5 + s * (Sb6 + s * Sb7))))));
            }
            r = Math.Exp(-ax * ax - 0.5625 + R / S);
            if (x >= 0)
                return One - r / ax;
            return r / ax - One;
        }
        #endregion

        /// <summary>
        ///Cumulative normal distribution function
        ///Given x it provides an approximation to the
        ///integral of the gaussian normal distribution:
        ///formula here ...
        ///    
        ///For this implementation see M. Abramowitz and I. Stegun,
        ///Handbook of Mathematical Functions,
        ///Dover Publications, New York (1972)
        /// </summary>
        public static double Cumulative(double z)
        {
            double result = 0.5 * (1.0 + Erf(z * MathConsts.Sqrt2));
            if (result <= 1e-8)
            {
                //todo : investigate the threshold level
                // Asymptotic expansion for very negative z following (26.2.12)
                // on page 408 in M. Abramowitz and A. Stegun,
                // Pocketbook of Mathematical Functions, ISBN 3-87144818-4.
                double sum = 1.0,
                    zsqr = z * z,
                    i = 1.0,
                    g = 1.0,
                    x,
                    y,
                    a = Double.MaxValue,
                    lasta;
                do
                {
                    lasta = a;
                    x = (4.0 * i - 3.0) / zsqr;
                    y = x * ((4.0 * i - 1) / zsqr);
                    a = g * (x - y);
                    sum -= a;
                    g *= y;
                    ++i;
                    a = Math.Abs(a);
                } while (lasta > a && a >= Math.Abs(sum * DoubleUtils.Epsilon));
                result = -Density(z) / z * sum;
            }
            return result;
        }

        /// <summary>
        /// Density of normal distribution
        /// </summary>
        public static double Density(double z)
        {
            return MathConsts.InvSqrtTwoPi * Math.Exp(-0.5 * z * z);
        }

        #region Cumulative Inverse Const
        // Coefficients for the rational approximation.
        private const double A1 = -3.969683028665376e+01;
        private const double A2 = 2.209460984245205e+02;
        private const double A3 = -2.759285104469687e+02;
        private const double A4 = 1.383577518672690e+02;
        private const double A5 = -3.066479806614716e+01;
        private const double A6 = 2.506628277459239e+00;

        private const double B1 = -5.447609879822406e+01;
        private const double B2 = 1.615858368580409e+02;
        private const double B3 = -1.556989798598866e+02;
        private const double B4 = 6.680131188771972e+01;
        private const double B5 = -1.328068155288572e+01;

        private const double C1 = -7.784894002430293e-03;
        private const double C2 = -3.223964580411365e-01;
        private const double C3 = -2.400758277161838e+00;
        private const double C4 = -2.549732539343734e+00;
        private const double C5 = 4.374664141464968e+00;
        private const double C6 = 2.938163982698783e+00;

        private const double D1 = 7.784695709041462e-03;
        private const double D2 = 3.224671290700398e-01;
        private const double D3 = 2.445134137142996e+00;
        private const double D4 = 3.754408661907416e+00;

        // Limits of the approximation regions
        private const double XLow = 0.02425;
        private const double XHigh = 1.0 - XLow;
        #endregion
        /// <summary>
        /// Inverse cumulative normal distribution function
        /// Acklam's approximation:
        /// by Peter J. Acklam, University of Oslo, Statistics Division.
        /// URL: http://home.online.no/~pjacklam/notes/invnorm/index.html
        /// </summary>
        public static double CumulativeInverse(double x, bool fullMachinePrecision = true)
        {
            if (x < 0.0 || x > 1.0)
            {
                // try to recover if due to numerical error
                if (DoubleUtils.MachineEquality(x, 1.0))
                {
                    x = 1.0;
                }
                else if (Math.Abs(x) < DoubleUtils.Epsilon)
                {
                    x = 0.0;
                }
                else
                {
                    throw new ApplicationException("InverseGaussCDF(" + x + ") undefined: must be 0 < x < 1");
                }
            }

            double z;

            if (x < XLow)
            {
                z = Math.Sqrt(-2.0 * Math.Log(x));
                z = (((((C1 * z + C2) * z + C3) * z + C4) * z + C5) * z + C6) /
                    ((((D1 * z + D2) * z + D3) * z + D4) * z + 1.0);
            }
            else if (x <= XHigh)
            {
                z = x - 0.5;
                double r = z * z;
                z = (((((A1 * r + A2) * r + A3) * r + A4) * r + A5) * r + A6) * z /
                    (((((B1 * r + B2) * r + B3) * r + B4) * r + B5) * r + 1.0);
            }
            else
            {
                z = Math.Sqrt(-2.0 * Math.Log(1.0 - x));
                z = -(((((C1 * z + C2) * z + C3) * z + C4) * z + C5) * z + C6) /
                    ((((D1 * z + D2) * z + D3) * z + D4) * z + 1.0);
            }


            // The relative error of the approximation has absolute value less
            // than 1.15e-9.  One iteration of Halley's rational method (third
            // order) gives full machine precision.
            if (fullMachinePrecision)
            {
                // error (Cumulative(z) - x) divided by the cumulative's derivative
                double r = (Cumulative(z) - x) * MathConsts.SqrtTwoPi * Math.Exp(0.5 * z * z);
                //  Halley's method
                z -= r / (1 + 0.5 * z * r);
            }

            return z;
        }
    }

}
