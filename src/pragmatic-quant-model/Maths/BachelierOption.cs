using System;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.Maths
{
    public static class BachelierOption
    {
        #region private methods
        private static double NormalizedCall(double d)
        {
            return Math.Exp(-0.5 * d * d) * (MathConsts.InvSqrtTwoPi - 0.5 * d * ErrorFunctions.Erfcx(MathConsts.InvSqrt2 * d));
        }
        #endregion

        /// <summary>
        /// Bachelier formulae for call and put option
        /// </summary>
        /// <param name="f">forward</param>
        /// <param name="k">strike</param>
        /// <param name="sigma">volatility</param>
        /// <param name="t">maturity</param>
        /// <param name="q"> 1 for call, -1 for put </param>
        /// <returns></returns>
        public static double Price(double f, double k, double sigma, double t, double q)
        {
            double intrinsic = Math.Abs(Math.Max((q < 0 ? k - f : f - k), 0.0));
            // Map in-the-money to out-of-the-money
            if (q * (f - k) > 0.0)
                return intrinsic + Price(f, k, sigma, t, -q);
            var v = sigma * Math.Sqrt(t);
            var d = (k - f) / v;
            return Math.Max(intrinsic, v * NormalizedCall(q < 0.0 ? -d : d));
        }

        /// <summary>
        /// Bachelier model second order greeks
        /// Gamma : d^2P / df^2
        /// Vega : dP / dsigma
        /// Vanna : dP^2 / dsigma df
        /// Vomma : dP^2 / dsigma^2
        /// </summary>
        /// <param name="f">forward</param>
        /// <param name="k">strike</param>
        /// <param name="t">maturity</param>
        /// <param name="sigma">volatility</param>
        /// <param name="gamma"> output option gamma</param>
        /// <param name="vega"> output option vega  </param>
        /// <param name="vanna"> output option vanna  </param>
        /// <param name="vomma"> output option vomma  </param>
        public static void Greeks(double f, double k, double t, double sigma,
                                  out double gamma, out double vega, out double vanna, out double vomma)
        {
            double sqrtMat = Math.Sqrt(t);
            double dev = (k - f) / sigma;

            gamma = MathConsts.InvSqrtTwoPi * Math.Exp(-0.5 * dev * dev / t) / (sigma * sqrtMat);
            vega = sigma * t * gamma;
            vanna = dev * gamma;
            vomma = dev * vanna;
        }
    }
}