using System;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.Maths
{
    public static class RootUtils
    {
        #region private methods
        private static bool Bracket(Func<double, double> f, double x1, double x2,                                   
                                    out double xa, out double fa,
                                    out double xb, out double fb)
        {
            const int NTRY = 50;
            const double FACTOR = 1.6;

            if (DoubleUtils.MachineEquality(x1, x2))
                throw new Exception("Bracket : bad initial range");
            xa = x1;
            xb = x2;
            fa = f(xa);
            fb = f(xb);

            double last = double.NaN;
            double lastf = double.NaN;

            for (int j = 0; j < NTRY; j++)
            {
                if (fa * fb < 0.0)
                {
                    if (lastf * fa < 0.0)
                    {
                        xb = last;
                        fb = lastf;
                    }
                    if (lastf * fb < 0.0)
                    {
                        xa = last;
                        fa = lastf;
                    }
                    return true;
                }
                if (Math.Abs(fa) < Math.Abs(fb))
                {
                    last = xa;
                    lastf = fa;
                    xa += FACTOR * (xa - xb);
                    fa = f(xa);
                }
                else
                {
                    last = xb;
                    lastf = fb;
                    xb += FACTOR * (xb - xa);
                    fb = f(xb);
                }
            }
            return false;
        }
        #endregion

        /// <summary>
        /// Brenth algorithm from scipy
        /// </summary>
        public static double Brenth(Func<double, double> f, 
                                    double xa, double fa, double xb, double fb,
                                    double xtol, double rtol, int maxIter,
                                    out int funcalls, out int iterations)
        {
            double xpre = xa;
            double xcur = xb;
            double fpre = fa;
            double fcur = fb;
            funcalls = 0;
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
        /// <param name="funcalls"> nb func call</param>
        /// <param name="iterations">nb algo iterations</param>
        /// <returns></returns>
        public static double Brenth(Func<double, double> f, double xa, double xb,
                                    double xtol, double rtol, int maxIter,
                                    out int funcalls, out int iterations)
        {
            var root= Brenth(f, xa, f(xa), xb, f(xb), xtol, rtol, maxIter, out funcalls, out iterations);
            funcalls += 2;
            return root;
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
        
        public static double BrentWithBracket(Func<double, double> f, double x1, double x2,
                                              double xtol, double rtol, int maxIter = 100)
        {
            double xa, fa, xb, fb;
            if (!Bracket(f, x1, x2, out xa, out fa, out xb, out fb))
                throw new Exception("Failed to bracket root !");

            int funcalls, iterations;
            return Brenth(f, xa, fa, xb, fb, xtol, rtol, maxIter, out funcalls, out iterations);
        }

        /// <summary>
        /// Root bracketing algorithm
        /// </summary>
        /// <param name="f">function to bracket</param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="xa"></param>
        /// <param name="xb"></param>
        /// <returns></returns>
        public static bool Bracket(Func<double, double> f, double x1, double x2,
                                   out double xa, out double xb)
        {
            double fa, fb;
            return Bracket(f, x1, x2, out xa, out fa, out xb, out fb);
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
