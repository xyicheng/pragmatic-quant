//
// Original Fortran code taken from http://www.netlib.org/specfun/erf, compiled with f2c, and adapted by hand.
//
// Created with command line f2c -C++ -c -a -krd -r8 cody_erf.f
//
// Translated by f2c (version 20100827).
//
//
// This source code resides at www.jaeckel.org/LetsBeRational.7z .
//
// ======================================================================================
// WARRANTY DISCLAIMER
// The Software is provided "as is" without warranty of any kind, either express or implied,
// including without limitation any implied warranties of condition, uninterrupted use,
// merchantability, fitness for a particular purpose, or non-infringement.
// ======================================================================================
//

using System;

namespace pragmatic_quant_model.Maths
{
    public static class ErrorFunctions
    {
        #region private const
        private static readonly double[] A =
        {
            3.1611237438705656, 113.864154151050156, 377.485237685302021,
            3209.37758913846947, 0.185777706184603153
        };
        private static readonly double[] B =
        {
            23.6012909523441209, 244.024637934444173, 1282.61652607737228,
            2844.23683343917062
        };
        private static readonly double[] C =
        {
            0.564188496988670089, 8.88314979438837594, 66.1191906371416295, 298.635138197400131, 881.95222124176909,
            1712.04761263407058, 2051.07837782607147, 1230.33935479799725, 2.15311535474403846e-8
        };
        private static readonly double[] D =
        {
            15.7449261107098347, 117.693950891312499, 537.181101862009858, 1621.38957456669019, 3290.79923573345963,
            4362.61909014324716, 3439.36767414372164, 1230.33935480374942
        };
        private static readonly double[] P =
        {
            0.305326634961232344, 0.360344899949804439, .125781726111229246, .0160837851487422766,
            6.58749161529837803e-4,
            .0163153871373020978
        };
        private static readonly double[] Q =
        {
            2.56852019228982242, 1.87295284992346047, .527905102951428412,
            .0605183413124413191, .00233520497626869185
        };

        private const double Zero = 0.0;
        private const double Half = 0.5;
        private const double One = 1.0;
        private const double Two = 2.0;
        private const double Four = 4.0;
        private const double Sqrpi = 0.56418958354775628695;
        private const double Thresh = 0.46875;
        private const double Sixten = 16.0;

        private const double Xinf = 1.79e308;
        private const double Xneg = -26.628;
        private const double Xsmall = 1.11e-16;
        private const double Xbig = 26.543;
        private const double Xhuge = 6.71e7;
        private const double Xmax = 2.53e307;
        #endregion
        #region private methods
        private static double d_int(double x)
        {
            return ((x > 0) ? Math.Floor(x) : -Math.Floor(-x));
        }
        private static double Calerf(double x, int jint)
        {
            #region description
            /* ------------------------------------------------------------------ */
            /* This packet evaluates  erf(x),  erfc(x),  and  exp(x*x)*erfc(x) */
            /*   for a real argument  x.  It contains three FUNCTION type */
            /*   subprograms: ERF, ERFC, and ERFCX (or DERF, DERFC, and DERFCX), */
            /*   and one SUBROUTINE type subprogram, CALERF.  The calling */
            /*   statements for the primary entries are: */
            /*                   Y=ERF(X)     (or   Y=DERF(X)), */
            /*                   Y=ERFC(X)    (or   Y=DERFC(X)), */
            /*   and */
            /*                   Y=ERFCX(X)   (or   Y=DERFCX(X)). */
            /*   The routine  CALERF  is intended for internal packet use only, */
            /*   all computations within the packet being concentrated in this */
            /*   routine.  The function subprograms invoke  CALERF  with the */
            /*   statement */
            /*          CALL CALERF(ARG,RESULT,JINT) */
            /*   where the parameter usage is as follows */
            /*      Function                     Parameters for CALERF */
            /*       call              ARG                  Result          JINT */
            /*     ERF(ARG)      ANY REAL ARGUMENT         ERF(ARG)          0 */
            /*     ERFC(ARG)     ABS(ARG) .LT. XBIG        ERFC(ARG)         1 */
            /*     ERFCX(ARG)    XNEG .LT. ARG .LT. XMAX   ERFCX(ARG)        2 */
            /*   The main computation evaluates near-minimax approximations */
            /*   from "Rational Chebyshev approximations for the error function" */
            /*   by W. J. Cody, Math. Comp., 1969, PP. 631-638.  This */
            /*   transportable program uses rational functions that theoretically */
            /*   approximate  erf(x)  and  erfc(x)  to at least 18 significant */
            /*   decimal digits.  The accuracy achieved depends on the arithmetic */
            /*   system, the compiler, the intrinsic functions, and proper */
            /*   selection of the machine-dependent constants. */
            /* ******************************************************************* */
            /* ******************************************************************* */
            /* Explanation of machine-dependent constants */
            /*   XMIN   = the smallest positive floating-point number. */
            /*   XINF   = the largest positive finite floating-point number. */
            /*   XNEG   = the largest negative argument acceptable to ERFCX; */
            /*            the negative of the solution to the equation */
            /*            2*exp(x*x) = XINF. */
            /*   XSMALL = argument below which erf(x) may be represented by */
            /*            2*x/sqrt(pi)  and above which  x*x  will not underflow. */
            /*            A conservative value is the largest machine number X */
            /*            such that   1.0 + X = 1.0   to machine precision. */
            /*   XBIG   = largest argument acceptable to ERFC;  solution to */
            /*            the equation:  W(x) * (1-0.5/x**2) = XMIN,  where */
            /*            W(x) = exp(-x*x)/[x*sqrt(pi)]. */
            /*   XHUGE  = argument above which  1.0 - 1/(2*x*x) = 1.0  to */
            /*            machine precision.  A conservative value is */
            /*            1/[2*sqrt(XSMALL)] */
            /*   XMAX   = largest acceptable argument to ERFCX; the minimum */
            /*            of XINF and 1/[sqrt(pi)*XMIN]. */
            // The numbers below were preselected for IEEE .
            #endregion
            double del, ysq, xden, xnum, result;
            double y = Math.Abs(x);
            if (y <= Thresh)
            {
                ysq = Zero;
                if (y > Xsmall)
                {
                    ysq = y * y;
                }
                xnum = A[4] * ysq;
                xden = ysq;
                for (int i = 1; i <= 3; ++i)
                {
                    xnum = (xnum + A[i - 1]) * ysq;
                    xden = (xden + B[i - 1]) * ysq;
                }
                result = x * (xnum + A[3]) / (xden + B[3]);
                if (jint != 0)
                {
                    result = One - result;
                }
                if (jint == 2)
                {
                    result = Math.Exp(ysq) * result;
                }
                goto L800;
            }
            if (y <= Four)
            {
                xnum = C[8] * y;
                xden = y;
                for (int i = 1; i <= 7; ++i)
                {
                    xnum = (xnum + C[i - 1]) * y;
                    xden = (xden + D[i - 1]) * y;
                }
                result = (xnum + C[7]) / (xden + D[7]);
                if (jint != 2)
                {
                    double d1 = y * Sixten;
                    ysq = d_int(d1) / Sixten;
                    del = (y - ysq) * (y + ysq);
                    d1 = Math.Exp(-ysq * ysq) * Math.Exp(-del);
                    result = d1 * result;
                }
            }
            else
            {
                result = Zero;
                if (y >= Xbig)
                {
                    if (jint != 2 || y >= Xmax)
                    {
                        goto L300;
                    }
                    if (y >= Xhuge)
                    {
                        result = Sqrpi / y;
                        goto L300;
                    }
                }
                ysq = One / (y * y);
                xnum = P[5] * ysq;
                xden = ysq;
                for (int i = 1; i <= 4; ++i)
                {
                    xnum = (xnum + P[i - 1]) * ysq;
                    xden = (xden + Q[i - 1]) * ysq;
                }
                result = ysq * (xnum + P[4]) / (xden + Q[4]);
                result = (Sqrpi - result) / y;
                if (jint != 2)
                {
                    double d1 = y * Sixten;
                    ysq = d_int(d1) / Sixten;
                    del = (y - ysq) * (y + ysq);
                    d1 = Math.Exp(-ysq * ysq) * Math.Exp(-del);
                    result = d1 * result;
                }
            }

            L300:
            if (jint == 0)
            {
                result = (Half - result) + Half;
                if (x < Zero)
                {
                    result = -(result);
                }
            }
            else if (jint == 1)
            {
                if (x < Zero)
                {
                    result = Two - result;
                }
            }
            else
            {
                if (x < Zero)
                {
                    if (x < Xneg)
                    {
                        result = Xinf;
                    }
                    else
                    {
                        double d1 = x * Sixten;
                        ysq = d_int(d1) / Sixten;
                        del = (x - ysq) * (x + ysq);
                        y = Math.Exp(ysq * ysq) * Math.Exp(del);
                        result = y + y - result;
                    }
                }
            }
            L800:
            return result;
        }
        #endregion

        /// <summary>
        /// This method computes approximate values for erf(x).
        /// Program from "Rational Chebyshev approximations for the error function" 
        ///   by W. J. Cody, Math. Comp., 1969, PP. 631-638.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Erf(double x)
        {
            return Calerf(x, 0);
        }
        
        /// <summary>
        /// This method computes approximate values for erfc(x).
        /// Program from "Rational Chebyshev approximations for the error function" 
        ///   by W. J. Cody, Math. Comp., 1969, PP. 631-638.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Erfc(double x)
        {
            return Calerf(x, 1);
        }
        
        /// <summary>
        /// This method computes approximate values for exp(x*x) * erfc(x).
        /// Program from "Rational Chebyshev approximations for the error function" 
        ///   by W. J. Cody, Math. Comp., 1969, PP. 631-638.
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static double Erfcx(double x)
        {
            return Calerf(x, 2);
        }
    }
}