using System;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.Maths
{
    public static class RootUtils
    {
        /// <summary>
        /// Brenth algorithm from scipy
        /// </summary>
        /// <param name="f"></param>
        /// <param name="xa"></param>
        /// <param name="xb"></param>
        /// <param name="xtol"></param>
        /// <param name="rtol"></param>
        /// <param name="maxIter"></param>
        /// <param name="funcalls"> nb func call</param>
        /// <param name="iterations">nb algo iterations</param>
        /// <returns></returns>
        public static double Brenth(Func<double, double> f, double xa, double xb,
                                    double xtol, double rtol, int maxIter,
                                    out int funcalls, out int iterations)
        {
            double xpre = xa;
            double xcur = xb;
            double fpre = f(xpre);
            double fcur = f(xcur);
            funcalls = 2;
            iterations = 0;

            if (fpre * fcur > 0)
                throw new Exception("Brent : root must be bracketed");
            
            if (DoubleUtils.EqualZero(fpre)) return xpre;
            if (DoubleUtils.EqualZero(fcur)) return xcur;

            double xblk = 0.0, fblk = 0.0, spre = 0.0, scur = 0.0;
            for (int i = 0; i < maxIter; i++)
            {
                iterations++;
                if (fpre * fcur < 0)
                {
                    xblk = xpre;
                    fblk = fpre;
                    spre = scur = xcur - xpre;
                }
                if (Math.Abs(fblk) < Math.Abs(fcur))
                {
                    xpre = xcur;
                    xcur = xblk;
                    xblk = xpre;
                    fpre = fcur;
                    fcur = fblk;
                    fblk = fpre;
                }

                double tol = xtol + rtol * Math.Abs(xcur);
                double sbis = (xblk - xcur) / 2.0;
                if (DoubleUtils.EqualZero(fcur) || Math.Abs(sbis) < tol)
                    return xcur;

                if (Math.Abs(spre) > tol && Math.Abs(fcur) < Math.Abs(fpre))
                {
                    double stry;
                    if (DoubleUtils.MachineEquality(xpre, xblk))
                    {
                        // interpolate
                        stry = -fcur * (xcur - xpre) / (fcur - fpre);
                    }
                    else
                    {
                        // extrapolate
                        double dpre = (fpre - fcur) / (xpre - xcur);
                        double dblk = (fblk - fcur) / (xblk - xcur);
                        stry = -fcur * (fblk - fpre) / (fblk * dpre - fpre * dblk);
                    }

                    if (2.0 * Math.Abs(stry) < Math.Min(Math.Abs(spre), 3.0 * Math.Abs(sbis) - tol))
                    {
                        // accept step
                        spre = scur;
                        scur = stry;
                    }
                    else
                    {
                        // bisect 
                        spre = sbis;
                        scur = sbis;
                    }
                }
                else
                {
                    // bisect 
                    spre = sbis;
                    scur = sbis;
                }

                xpre = xcur;
                fpre = fcur;
                if (Math.Abs(scur) > tol)
                    xcur += scur;
                else
                    xcur += (sbis > 0.0 ? tol : -tol);

                fcur = f(xcur);
                funcalls++;
            }
            throw new Exception("Brent : max iteration excedeed");
        }

        /// <summary>
        /// Brenth algorithm from scipy
        /// </summary>
        /// <param name="f"></param>
        /// <param name="xa"></param>
        /// <param name="xb"></param>
        /// <param name="xtol"></param>
        /// <param name="rtol"></param>
        /// <param name="maxIter"></param>
        /// <returns></returns>
        public static double Brenth(Func<double, double> f, double xa, double xb,
                                    double xtol, double rtol, int maxIter = 100)
        {
            int funcalls, iterations;
            return Brenth(f, xa, xb, xtol, rtol, maxIter, out funcalls, out iterations);
        }

        /// <summary>
        /// Root bracketing algorithm
        /// </summary>
        /// <param name="f">function to bracket</param>
        /// <param name="xa"></param>
        /// <param name="xb"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <returns></returns>
        public static bool Bracket(Func<double, double> f, double xa, double xb,
                                   out double x1, out double x2)
        {
            const int NTRY = 50;
            const double FACTOR = 1.6;

            if (DoubleUtils.MachineEquality(xa, xb))
                throw new Exception("Bracket : bad initial range");
            x1 = xa;
            x2 = xb;
            double f1 = f(x1);
            double f2 = f(x2);
            for (int j = 0; j < NTRY; j++)
            {
                if (f1 * f2 < 0.0) return true;
                if (Math.Abs(f1) < Math.Abs(f2))
                {
                    x1 += FACTOR * (x1 - x2);
                    f1 = f(x1);
                }
                else
                {
                    x2 += FACTOR * (x2 - x1);
                    f2 = f(x2);
                }
            }
            return false;
        }

        public static double TrinomRoot(double a, double b, double c,
                                        double fa, double fb, double fc)
        {
            double s1 = a * fb * fc / ((fa - fb) * (fa - fc));
            double s2 = b * fa * fc / ((fb - fa) * (fb - fc));
            double s3 = c * fa * fb / ((fc - fa) * (fc - fb));
            return s1 + s2 + s3;
        }
    }
}
