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
        private static double NormalizedImpliedVol(double x, double price, double q)
        {
            // Subtract intrinsic.
            if (q * x < 0.0)
            {
                price = Math.Abs(Math.Max(price - Math.Abs(Math.Max((q < 0.0 ? -x : x), 0.0)), 0.0));
                q = -q;
            }
            // Map puts to calls
            if (q < 0.0)
            {
                x = -x;
            }


            var z = x / price;
            var u = MathConsts.InvSqrtTwoPi * Math.Log(1.0 + x / price);

            if (Math.Abs(u) < 2.84431233111)
            {
                var uSquare = u * u;
                var r = (1.0 + uSquare * (0.282379182817 + uSquare * (0.0247121443305 + uSquare * (0.000710539797155 + uSquare * 4.4377233994E-06))))
                        /(0.999999999996 +uSquare * (0.258780407354 + uSquare * (0.0199703240022 + uSquare * (0.000467308655509 + uSquare * 1.82925792864E-6))));

                if (Math.Abs(z * z * z) < DoubleUtils.MachineEpsilon)
                {
                    return MathConsts.SqrtTwoPi * price * (1.0 + 0.5 * z - 0.0833333333333 * z * z) * r;
                }
                return (x / u) * r;
            }

            var usqrt = Math.Sqrt(u);
            if (u < 8.71847556468)
            {
                var r = (1.0 + usqrt * (-1.52385551571 + usqrt * (1.49840194722 + usqrt * (-0.560753569135 + usqrt * (0.00723314207584 + usqrt * 0.0780273038347)))));
                r /= -0.744369490247 + usqrt * (3.33498816743 + usqrt * (-5.25411804691 + usqrt * (4.64740978912 + usqrt * (-1.68629712489 + usqrt * (0.0153889969365 + usqrt * 0.174527680356)))));
                return x * r;
            }
            else
            {
                var r = (1.0 + usqrt * (-0.488033526269 + usqrt * (0.334544231232 + usqrt * (0.945257573157 + usqrt * (0.154446563144 + usqrt * 0.00419713331631)))));
                r /= -1.06246389343 + usqrt * (4.28056960745 + usqrt * (-3.6379814834 + usqrt * (-0.0366778945247 + usqrt * (2.08398893794 + usqrt * (0.34575195248 + usqrt * 0.00939770668811)))));
                return x * r;
            }
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
        /// Bachelier implied volatility from a given price
        /// </summary>
        /// <param name="price"> option price </param>
        /// <param name="f"> forward </param>
        /// <param name="k"> strike </param>
        /// <param name="t"> maturity </param>
        /// <param name="q"> 1 for call, -1 for put </param>
        /// <returns></returns>
        public static double ImpliedVol(double price, double f, double k, double t, double q)
        {
            double intrinsic = Math.Abs(Math.Max((q < 0.0 ? k - f : f - k), 0.0));
            if (price < intrinsic)
                return JaeckelBlackFormula.VolatilityValueToSignalPriceIsBelowIntrinsic;
            double x = k - f;
            if (q * x < 0.0)
            {
                price = Math.Abs(Math.Max(price - intrinsic, 0.0));
                x = -x;
                q = -q;
            }
            return NormalizedImpliedVol(x, price, q) / Math.Sqrt(t);
        }

        /// <summary>
        /// Bachelier model second order greeks
        /// Gamma : d^2P / df^2
        /// Vega : dP / dsigma
        /// Vanna : dP^2 / dsigmadf
        /// Vomma : dP^2 / dsigma^2
        /// </summary>
        /// <param name="f">forward</param>
        /// <param name="k">strike</param>
        /// <param name="t">maturity</param>
        /// <param name="sigma">volatility</param>
        /// <param name="gamma"> output option gamma</param>
        /// <param name="theta"> output option theta</param>
        /// <param name="vega"> output option vega  </param>
        /// <param name="vanna"> output option vanna  </param>
        /// <param name="vomma"> output option vomma  </param>
        public static void Greeks(double f, double k, double t, double sigma,
            out double gamma, out double theta, out double vega, out double vanna, out double vomma)
        {
            double sqrtMat = Math.Sqrt(t);
            double dev = (k - f) / sigma;

            gamma = MathConsts.InvSqrtTwoPi * Math.Exp(-0.5 * dev * dev / t) / (sigma * sqrtMat);
            theta = -0.5 * sigma * sigma * gamma;
            vega = sigma * t * gamma;
            vanna = dev * gamma;
            vomma = dev * vanna;
        }
    }
}