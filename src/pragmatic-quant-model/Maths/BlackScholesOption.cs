﻿using System;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.Maths
{
    public static class BlackScholesOption
    {
        /// <summary>
        /// Black-Scholes formulae for call and put option
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
            return Math.Max(intrinsic,
                Math.Sqrt(f * k) * JaeckelBlackFormula.NormalisedBlack(Math.Log(f / k), sigma * Math.Sqrt(t), q));
        }

        /// <summary>
        /// Black-Scholes formulae for digital call and put option
        /// </summary>
        /// <param name="f">forward</param>
        /// <param name="k">strike</param>
        /// <param name="vol">volatility</param>
        /// <param name="t">maturity</param>
        /// <param name="q">1 for digit call, -1 for digit pu</param>
        /// <param name="skew">(optional) derivative of volatility relative to strike</param>
        /// <returns></returns>
        public static double PriceDigit(double f, double k, double vol, double t, double q, double skew = 0.0)
        {
            double x = Math.Log(f / k);
            double s = vol * Math.Sqrt(t);
            double digit = JaeckelBlackFormula.NormalisedDigit(x, s, q);
            if (!DoubleUtils.EqualZero(skew))
            {
                double vega, temp;
                Greeks(f, k, t, vol, out temp, out temp, out vega, out temp, out temp);
                digit += (q > 0.0 ? -1.0 : 1.0) * skew * vega;
            }
            return digit;
        }
        
        /// <summary>
        /// Black implied volatility from a given price
        /// </summary>
        /// <param name="price"> option price </param>
        /// <param name="f"> forward </param>
        /// <param name="k"> strike </param>
        /// <param name="t"> maturity </param>
        /// <param name="q"> 1 for call, -1 for put </param>
        /// <param name="n"> number of householder solver iterations </param>
        /// <returns></returns>
        public static double ImpliedVol(double price, double f, double k, double t, double q, int n = 2)
        {
            double intrinsic = Math.Abs(Math.Max((q < 0.0 ? k - f : f - k), 0.0));
            if (price < intrinsic)
                return JaeckelBlackFormula.VolatilityValueToSignalPriceIsBelowIntrinsic;
            double max_price = (q < 0.0 ? k : f);
            if (price >= max_price)
                return JaeckelBlackFormula.VolatilityValueToSignalPriceIsAboveMaximum;
            double x = Math.Log(f / k);
            // Map in-the-money to out-of-the-money
            if (q * x > 0.0)
            {
                price = Math.Abs(Math.Max(price - intrinsic, 0.0));
                q = -q;
            }
            return
                JaeckelBlackFormula.UncheckedNormalisedImpliedVolatility(
                    price / Math.Sqrt(f * k), x, q, n) /
                Math.Sqrt(t);
        }
        
        /// <summary>
        /// Black model second order greeks
        /// Gamma : d^2P / df^2
        /// Theta : -dP / dt
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
            var timesqrt = Math.Sqrt(t);
            var stdDev = sigma * timesqrt;
            var d_plus = Math.Log(f / k) / stdDev + 0.5 * stdDev;
            var d_minus = d_plus - stdDev;

            var density = MathConsts.InvSqrtTwoPi * Math.Exp(-0.5 * d_plus * d_plus);
            gamma = density / f / stdDev;
            theta = -0.5 * f * f * sigma * sigma * gamma;
            vega = timesqrt * f * density;
            vanna = -d_minus / sigma * density;
            vomma = d_plus * d_minus / sigma * timesqrt * f * density;
        }
    }

    internal static class JaeckelBlackFormula
    {
        #region private const
        private static readonly double SqrtDblMin = Math.Sqrt(double.Epsilon);
        private static readonly double SqrtDblMax = Math.Sqrt(double.MaxValue);

        /// Set this to 0 if you want positive results for (positive) denormalized inputs, else to DBL_MIN.
        /// Note that you cannot achieve full machine accuracy from denormalized inputs!
        private const double Denormalization_Cutoff = 0;
        private const double Asymptotic_Expansion_Accuracy_Threshold = -10;
        private static readonly double SmallTExpansionOfNormalizedBlackThreshold = 2.0 * DoubleUtils.SixteenthRoot_Epsilon;
        #endregion
        #region private methods
        private static double normalised_intrinsic(double x, double q)
        {
            if (q * x <= 0)
                return 0.0;
            double x2 = x * x;
            if (x2 < 98 * DoubleUtils.FourthRoot_Epsilon)
                // The factor 98 is computed from last coefficient: √√92897280 = 98.1749
                return
                    Math.Abs(
                        Math.Max(
                            (q < 0 ? -1 : 1) * x *
                            (1 + x2 *
                             ((1.0 / 24.0) + x2 * ((1.0 / 1920.0) + x2 * ((1.0 / 322560.0) + (1.0 / 92897280.0) * x2))))
                            , 0.0));
            double b_max = Math.Exp(0.5 * x), one_over_b_max = 1 / b_max;
            return Math.Abs(Math.Max((q < 0 ? -1 : 1) * (b_max - one_over_b_max), 0.0));
        }
        private static double normalised_intrinsic_call(double x)
        {
            return normalised_intrinsic(x, 1);
        }
        /// <summary>
        /// Calculation of
        ///
        ///              b  =  Φ(h+t)·exp(h·t) - Φ(h-t)·exp(-h·t)
        ///
        ///                    exp(-(h²+t²)/2)
        ///                 =  --------------- ·  [ Y(h+t) - Y(h-t) ]
        ///                        √(2π)
        /// with
        ///           Y(z) := Φ(z)/φ(z)
        ///
        /// using an expansion of Y(h+t)-Y(h-t) for small t to twelvth order in t.
        /// Theoretically accurate to (better than) precision  ε = 2.23E-16  when  h l.t. 0
        /// and  t l.t. τ  with τ := 2·ε^(1/16) ≈ 0.21.  The main bottleneck for precision is the coefficient a:=1+h·Y(h) when |h|>1 .
        /// </summary>
        private static double small_t_expansion_of_normalized_black_call(double h, double t)
        {
            // Y(h) := Φ(h)/φ(h) = √(π/2)·erfcx(-h/√2)
            // a := 1+h·Y(h)  --- Note that due to h<0, and h·Y(h) -> -1 (from above) as h -> -∞, we also have that a>0 and a -> 0 as h -> -∞
            // w := t² , h2 := h²
            double a = 1 + h * (0.5 * MathConsts.SqrtTwoPi) * ErrorFunctions.Erfcx(-MathConsts.InvSqrt2 * h), w = t * t, h2 = h * h;
            double expansion = 2*t*(a+w*((-1+3*a+a*h2)/6+w*((-7+15*a+h2*(-1+10*a+a*h2))/120+w*((-57+105*a+h2*(-18+105*a+h2*(-1+21*a+a*h2)))/5040+w*((-561+945*a+h2*(-285+1260*a+h2*(-33+378*a+h2*(-1+36*a+a*h2))))/362880+w*((-6555+10395*a+h2*(-4680+17325*a+h2*(-840+6930*a+h2*(-52+990*a+h2*(-1+55*a+a*h2)))))/39916800+((-89055+135135*a+h2*(-82845+270270*a+h2*(-20370+135135*a+h2*(-1926+25740*a+h2*(-75+2145*a+h2*(-1+78*a+a*h2))))))*w)/6227020800.0))))));
            double b = MathConsts.InvSqrtTwoPi * Math.Exp((-0.5 * (h * h + t * t))) * expansion;
            return Math.Abs(Math.Max(b, 0.0));
        }
        ///<summary>
        /// Asymptotic expansion of
        ///
        ///              b  =  Φ(h+t)·exp(x/2) - Φ(h-t)·exp(-x/2)
        /// with
        ///              h  =  x/s   and   t  =  s/2
        /// which makes
        ///              b  =  Φ(h+t)·exp(h·t) - Φ(h-t)·exp(-h·t)
        ///
        ///                    exp(-(h²+t²)/2)
        ///                 =  ---------------  ·  [ Y(h+t) - Y(h-t) ]
        ///                        √(2π)
        /// with
        ///           Y(z) := Φ(z)/φ(z)
        ///
        /// for large negative (t-|h|) by the aid of Abramowitz & Stegun (26.2.12) where Φ(z) = φ(z)/|z|·[1-1/z^2+...].
        /// We define
        ///                     r
        ///         A(h,t) :=  --- · [ Y(h+t) - Y(h-t) ]
        ///                     t
        ///
        /// with r := (h+t)·(h-t) and give an expansion for A(h,t) in q:=(h/r)² expressed in terms of e:=(t/h)² .
        ///</summary>
        private static double asymptotic_expansion_of_normalized_black_call(double h, double t)
        {
            double e = (t / h) * (t / h), r = ((h + t) * (h - t)), q = (h / r) * (h / r);
            // 17th order asymptotic expansion of A(h,t) in q, sufficient for Φ(h) [and thus y(h)] to have relative accuracy of 1.64E-16 for h <= η  with  η:=-10.
            double asymptotic_expansion_sum = (2.0 +
                                               q *
                                               (-6.0E0 - 2.0 * e +
                                                3.0 * q *
                                                (1.0E1 + e * (2.0E1 + 2.0 * e) +
                                                 5.0 * q *
                                                 (-1.4E1 + e * (-7.0E1 + e * (-4.2E1 - 2.0 * e)) +
                                                  7.0 * q *
                                                  (1.8E1 + e * (1.68E2 + e * (2.52E2 + e * (7.2E1 + 2.0 * e))) +
                                                   9.0 * q *
                                                   (-2.2E1 +
                                                    e * (-3.3E2 + e * (-9.24E2 + e * (-6.6E2 + e * (-1.1E2 - 2.0 * e)))) +
                                                    1.1E1 * q *
                                                    (2.6E1 +
                                                     e *
                                                     (5.72E2 +
                                                      e *
                                                      (2.574E3 + e * (3.432E3 + e * (1.43E3 + e * (1.56E2 + 2.0 * e))))) +
                                                     1.3E1 * q *
                                                     (-3.0E1 +
                                                      e *
                                                      (-9.1E2 +
                                                       e *
                                                       (-6.006E3 +
                                                        e *
                                                        (-1.287E4 +
                                                         e * (-1.001E4 + e * (-2.73E3 + e * (-2.1E2 - 2.0 * e)))))) +
                                                      1.5E1 * q *
                                                      (3.4E1 +
                                                       e *
                                                       (1.36E3 +
                                                        e *
                                                        (1.2376E4 +
                                                         e *
                                                         (3.8896E4 +
                                                          e *
                                                          (4.862E4 +
                                                           e * (2.4752E4 + e * (4.76E3 + e * (2.72E2 + 2.0 * e))))))) +
                                                       1.7E1 * q *
                                                       (-3.8E1 +
                                                        e *
                                                        (-1.938E3 +
                                                         e *
                                                         (-2.3256E4 +
                                                          e *
                                                          (-1.00776E5 +
                                                           e *
                                                           (-1.84756E5 +
                                                            e *
                                                            (-1.51164E5 +
                                                             e * (-5.4264E4 + e * (-7.752E3 + e * (-3.42E2 - 2.0 * e)))))))) +
                                                        1.9E1 * q *
                                                        (4.2E1 +
                                                         e *
                                                         (2.66E3 +
                                                          e *
                                                          (4.0698E4 +
                                                           e *
                                                           (2.3256E5 +
                                                            e *
                                                            (5.8786E5 +
                                                             e *
                                                             (7.05432E5 +
                                                              e *
                                                              (4.0698E5 +
                                                               e * (1.08528E5 + e * (1.197E4 + e * (4.2E2 + 2.0 * e))))))))) +
                                                         2.1E1 * q *
                                                         (-4.6E1 +
                                                          e *
                                                          (-3.542E3 +
                                                           e *
                                                           (-6.7298E4 +
                                                            e *
                                                            (-4.90314E5 +
                                                             e *
                                                             (-1.63438E6 +
                                                              e *
                                                              (-2.704156E6 +
                                                               e *
                                                               (-2.288132E6 +
                                                                e *
                                                                (-9.80628E5 +
                                                                 e *
                                                                 (-2.01894E5 + e * (-1.771E4 + e * (-5.06E2 - 2.0 * e)))))))))) +
                                                          2.3E1 * q *
                                                          (5.0E1 +
                                                           e *
                                                           (4.6E3 +
                                                            e *
                                                            (1.0626E5 +
                                                             e *
                                                             (9.614E5 +
                                                              e *
                                                              (4.08595E6 +
                                                               e *
                                                               (8.9148E6 +
                                                                e *
                                                                (1.04006E7 +
                                                                 e *
                                                                 (6.53752E6 +
                                                                  e *
                                                                  (2.16315E6 +
                                                                   e * (3.542E5 + e * (2.53E4 + e * (6.0E2 + 2.0 * e))))))))))) +
                                                           2.5E1 * q *
                                                           (-5.4E1 +
                                                            e *
                                                            (-5.85E3 +
                                                             e *
                                                             (-1.6146E5 +
                                                              e *
                                                              (-1.77606E6 +
                                                               e *
                                                               (-9.37365E6 +
                                                                e *
                                                                (-2.607579E7 +
                                                                 e *
                                                                 (-4.01166E7 +
                                                                  e *
                                                                  (-3.476772E7 +
                                                                   e *
                                                                   (-1.687257E7 +
                                                                    e *
                                                                    (-4.44015E6 +
                                                                     e *
                                                                     (-5.9202E5 +
                                                                      e * (-3.51E4 + e * (-7.02E2 - 2.0 * e)))))))))))) +
                                                            2.7E1 * q *
                                                            (5.8E1 +
                                                             e *
                                                             (7.308E3 +
                                                              e *
                                                              (2.3751E5 +
                                                               e *
                                                               (3.12156E6 +
                                                                e *
                                                                (2.003001E7 +
                                                                 e *
                                                                 (6.919458E7 +
                                                                  e *
                                                                  (1.3572783E8 +
                                                                   e *
                                                                   (1.5511752E8 +
                                                                    e *
                                                                    (1.0379187E8 +
                                                                     e *
                                                                     (4.006002E7 +
                                                                      e *
                                                                      (8.58429E6 +
                                                                       e *
                                                                       (9.5004E5 +
                                                                        e * (4.7502E4 + e * (8.12E2 + 2.0 * e))))))))))))) +
                                                             2.9E1 * q *
                                                             (-6.2E1 +
                                                              e * (-8.99E3 +
                                                                   e * (-3.39822E5 +
                                                                        e * (-5.25915E6 +
                                                                             e * (-4.032015E7 +
                                                                                  e * (-1.6934463E8 +
                                                                                       e * (-4.1250615E8 +
                                                                                            e * (-6.0108039E8 +
                                                                                                 e * (-5.3036505E8 +
                                                                                                      e * (-2.8224105E8 +
                                                                                                           e * (-8.870433E7 +
                                                                                                                e * (-1.577745E7 +
                                                                                                                     e * (-1.472562E6 +
                                                                                                                          e *
                                                                                                                          (-6.293E4 +
                                                                                                                           e * (-9.3E2 - 2.0 * e)))))))))))))) +
                                                              3.1E1 * q *
                                                              (6.6E1 + e * (1.0912E4 +
                                                                            e * (4.74672E5 +
                                                                                 e * (8.544096E6 +
                                                                                      e * (7.71342E7 +
                                                                                           e * (3.8707344E8 +
                                                                                                e * (1.14633288E9 +
                                                                                                     e * (2.07431664E9 +
                                                                                                          e * (2.33360622E9 +
                                                                                                               e * (1.6376184E9 +
                                                                                                                    e * (7.0963464E8 +
                                                                                                                         e * (1.8512208E8 +
                                                                                                                              e * (2.7768312E7 +
                                                                                                                                   e * (2.215136E6 +
                                                                                                                                        e *
                                                                                                                                        (8.184E4 +
                                                                                                                                         e *
                                                                                                                                         (1.056E3 +
                                                                                                                                          2.0 * e)))))))))))))))
                                                               + 3.3E1 * (-7.0E1 + e * (-1.309E4 +
                                                                                        e * (-6.49264E5 +
                                                                                             e * (-1.344904E7 +
                                                                                                  e * (-1.4121492E8 +
                                                                                                       e * (-8.344518E8 +
                                                                                                            e * (-2.9526756E9 +
                                                                                                                 e * (-6.49588632E9 +
                                                                                                                      e * (-9.0751353E9 +
                                                                                                                           e * (-8.1198579E9 +
                                                                                                                                e * (-4.6399188E9 +
                                                                                                                                     e *
                                                                                                                                     (-1.6689036E9 +
                                                                                                                                      e *
                                                                                                                                      (-3.67158792E8 +
                                                                                                                                       e *
                                                                                                                                       (-4.707164E7 +
                                                                                                                                        e *
                                                                                                                                        (-3.24632E6 +
                                                                                                                                         e *
                                                                                                                                         (-1.0472E5 +
                                                                                                                                          e *
                                                                                                                                          (-1.19E3 -
                                                                                                                                           2.0 * e))))))))))))))))) *
                                                               q)))))))))))))))));
            double b = MathConsts.InvSqrtTwoPi * Math.Exp((-0.5 * (h * h + t * t))) * (t / r) * asymptotic_expansion_sum;
            return Math.Abs(Math.Max(b, 0.0));
        }
        /// <summary>
        ///     b(x,s)  =  Φ(x/s+s/2)·exp(x/2)  -   Φ(x/s-s/2)·exp(-x/2)
        ///             =  Φ(h+t)·exp(x/2)      -   Φ(h-t)·exp(-x/2)
        /// with
        ///              h  =  x/s   and   t  =  s/2
        /// </summary>
        private static double normalized_black_call_using_norm_cdf(double x, double s)
        {
            double h = x / s, t = 0.5 * s;
            double b_max = Math.Exp(0.5 * x);
            double b = NormalDistribution.Cumulative(h + t) * b_max - NormalDistribution.Cumulative(h - t) / b_max;
            return Math.Abs(Math.Max(b, 0.0));
        }
        private static double normalised_black_call_using_erfcx(double h, double t)
        {
            // Given h = x/s and t = s/2, the normalised Black function can be written as
            //
            //     b(x,s)  =  Φ(x/s+s/2)·exp(x/2)  -   Φ(x/s-s/2)·exp(-x/2)
            //             =  Φ(h+t)·exp(h·t)      -   Φ(h-t)·exp(-h·t) .                     (*)
            //
            // It is mentioned in section 4 (and discussion of figures 2 and 3) of George Marsaglia's article "Evaluating the
            // Normal Distribution" (available at http://www.jstatsoft.org/v11/a05/paper) that the error of any cumulative normal
            // function Φ(z) is dominated by the hardware (or compiler implementation) accuracy of exp(-z²/2) which is not
            // reliably more than 14 digits when z is large. The accuracy of Φ(z) typically starts coming down to 14 digits when
            // z is around -8. For the (normalised) Black function, as above in (*), this means that we are subtracting two terms
            // that are each products of terms with about 14 digits of accuracy. The net result, in each of the products, is even
            // less accuracy, and then we are taking the difference of these terms, resulting in even less accuracy. When we are
            // using the asymptotic expansion asymptotic_expansion_of_normalized_black_call() invoked in the second branch at the
            // beginning of this function, we are using only *one* exponential instead of 4, and this improves accuracy. It
            // actually improves it a bit more than you would expect from the above logic, namely, almost the full two missing
            // digits (in 64 bit IEEE floating point).  Unfortunately, going higher order in the asymptotic expansion will not
            // enable us to gain more accuracy (by extending the range in which we could use the expansion) since the asymptotic
            // expansion, being a divergent series, can never gain 16 digits of accuracy for z=-8 or just below. The best you can
            // get is about 15 digits (just), for about 35 terms in the series (26.2.12), which would result in an prohibitively
            // long expression in function asymptotic expansion asymptotic_expansion_of_normalized_black_call(). In this last branch,
            // here, we therefore take a different tack as follows.
            //     The "scaled complementary error function" is defined as erfcx(z) = exp(z²)·erfc(z). Cody's implementation of this
            // function as published in "Rational Chebyshev approximations for the error function", W. J. Cody, Math. Comp., 1969, pp.
            // 631-638, uses rational functions that theoretically approximates erfcx(x) to at least 18 significant decimal digits,
            // *without* the use of the exponential function when x>4, which translates to about z<-5.66 in Φ(z). To make use of it,
            // we write
            //             Φ(z) = exp(-z²/2)·erfcx(-z/√2)/2
            //
            // to transform the normalised black function to
            //
            //   b   =  ½ · exp(-½(h²+t²)) · [ erfcx(-(h+t)/√2) -  erfcx(-(h-t)/√2) ]
            //
            // which now involves only one exponential, instead of three, when |h|+|t| > 5.66 , and the difference inside the
            // square bracket is between the evaluation of two rational functions, which, typically, according to Marsaglia,
            // retains the full 16 digits of accuracy (or just a little less than that).
            //
            double b = 0.5 * Math.Exp(-0.5 * (h * h + t * t)) *
                       (ErrorFunctions.Erfcx(-MathConsts.InvSqrt2 * (h + t))
                        - ErrorFunctions.Erfcx(-MathConsts.InvSqrt2 * (h - t)));
            return Math.Abs(Math.Max(b, 0.0));
        }
        #endregion

        /// <summary>
        /// Normalized Black-Scholes formula
        /// by Peter Jaeckel.
        /// URL: http://www.jaeckel.org/LetsBeRational.7z
        /// </summary>
        private static double NormalisedBlackCall(double x, double s)
        {
            if (x > 0.0)
                return normalised_intrinsic_call(x) + NormalisedBlackCall(-x, s);
            double ax = Math.Abs(x);
            if (s <= ax * Denormalization_Cutoff)
                return normalised_intrinsic_call(x);
            // Denote h := x/s and t := s/2.
            // We evaluate the condition |h|>|η|, i.e., h<η  &&  t < τ+|h|-|η|  avoiding any divisions by s , where η = asymptotic_expansion_accuracy_threshold  and τ = small_t_expansion_of_normalized_black_threshold .
            if (x < s * Asymptotic_Expansion_Accuracy_Threshold &&
                0.5 * s * s + x < s * (SmallTExpansionOfNormalizedBlackThreshold + Asymptotic_Expansion_Accuracy_Threshold))
                // Region 1.
                return asymptotic_expansion_of_normalized_black_call(x / s, 0.5 * s);
            if (0.5 * s < SmallTExpansionOfNormalizedBlackThreshold)
                // Region 2.
                return small_t_expansion_of_normalized_black_call(x / s, 0.5 * s);
            // When b is more than, say, about 85% of b_max=exp(x/2), then b is dominated by the first of the two terms in the Black formula, and we retain more accuracy by not attempting to combine the two terms in any way.
            // We evaluate the condition h+t>0.85  avoiding any divisions by s.
            if (x + 0.5 * s * s > s * 0.85)
                // Region 3.
                return normalized_black_call_using_norm_cdf(x, s);
            // Region 4.
            return normalised_black_call_using_erfcx(x / s, 0.5 * s);
        }

        /// <summary>
        /// Normalized Black-Scholes formula
        /// by Peter Jaeckel.
        /// URL: http://www.jaeckel.org/LetsBeRational.7z
        /// </summary>
        /// <param name="x">log moneyness</param>
        /// <param name="s">standard deviation</param>
        /// <param name="q"> 1 for call, -1 for put </param>
        /// <returns></returns>
        public static double NormalisedBlack(double x, double s, double q)
        {
            return NormalisedBlackCall((q < 0 ? -x : x), s); /* Reciprocal-strike call-put equivalence */
        }

        public static double NormalisedVega(double x, double s)
        {
            double ax = Math.Abs(x);
            return (ax <= 0)
                ? MathConsts.InvSqrtTwoPi * Math.Exp(-0.125 * s * s)
                : ((s <= 0 || s <= ax * SqrtDblMin) ? 0 : MathConsts.InvSqrtTwoPi * Math.Exp(-0.5 * ((x * x / (s * s)) + (0.25 * s * s))));
        }

        public static double NormalisedDigit(double x, double s, double q)
        {
            double d2 = x / s - 0.5 * s;
            return NormalDistribution.Cumulative(q > 0.0 ? d2 : -d2);
        }
        
        #region private methods
        private static double householder_factor(double newton, double halley, double hh3)
        {
            return (1 + 0.5 * halley * newton) / (1.0 + newton * (halley + hh3 * newton / 6.0));
        }
        private static void compute_f_upper_map_and_first_two_derivatives(double x, double s, out double f, out double fp, out double fpp)
        {
            f = NormalDistribution.Cumulative(-0.5 * s);
            if (Math.Abs(x) < Denormalization_Cutoff)
            {
                fp = -0.5;
                fpp = 0;
            }
            else
            {
                double w = (x * x / (s * s));
                fp = -0.5 * Math.Exp(0.5 * w);
                fpp = 1.253314137315500251207882642405522626503493370305 * Math.Exp(w + 0.125 * s * s) * w / s;
            }
        }
        private static void compute_f_lower_map_and_first_two_derivatives(double x, double s, out double f, out double fp, out double fpp)
        {
            double ax = Math.Abs(x),
                z = MathConsts.SqrtOneOverThree * ax / s,
                y = z * z,
                s2 = s * s,
                Phi = NormalDistribution.Cumulative(-z),
                phi = NormalDistribution.Cumulative(z);
            fpp = MathConsts.PiOverSix * y / (s2 * s) * Phi * (8.0 * MathConsts.SqrtThree * s * ax + (3.0 * s2 * (s2 - 8.0) - 8.0 * x * x) * Phi / phi)
                  * Math.Exp(2.0 * y + 0.25 * s2);
            if (Math.Abs(s) < Denormalization_Cutoff)
            {
                fp = 1.0;
                f = 0.0;
            }
            else
            {
                double Phi2 = Phi * Phi;
                fp = MathConsts.TwoPi * y * Phi2 * Math.Exp(y + 0.125 * s * s);
                if (Math.Abs(x) < Denormalization_Cutoff)
                    f = 0.0;
                else
                    f = 1.209199576156145233729385505094770488189377498728 * ax * (Phi2 * Phi);
            }
        }
        private static double inverse_f_lower_map(double x, double f)
        {
            return Math.Abs(f) < Denormalization_Cutoff
                ? 0.0
                : Math.Abs(x / (MathConsts.SqrtThree *
                                NormalDistribution.CumulativeInverse(
                                    Math.Pow(f / (1.209199576156145233729385505094770488189377498728 * Math.Abs(x)), 1.0 / 3.0))
                    ));
        }
        #endregion
        // See http://en.wikipedia.org/wiki/Householder%27s_method for a detailed explanation of the third order Householder iteration.
        //
        // Given the objective function g(s) whose root x such that 0 = g(s) we seek, iterate
        //
        //     s_n+1  =  s_n  -  (g/g') · [ 1 - (g''/g')·(g/g') ] / [ 1 - (g/g')·( (g''/g') - (g'''/g')·(g/g')/6 ) ]
        //
        // Denoting  newton:=-(g/g'), halley:=(g''/g'), and hh3:=(g'''/g'), this reads
        //
        //     s_n+1  =  s_n  +  newton · [ 1 + halley·newton/2 ] / [ 1 + newton·( halley + hh3·newton/6 ) ]
        //
        //
        // NOTE that this function returns 0 when beta<intrinsic without any safety checks.
        //
        public static double UncheckedNormalisedImpliedVolatility(double beta, double x, double q, int n)
        {
            // Subtract intrinsic.
            if (q * x > 0.0)
            {
                beta = Math.Abs(Math.Max(beta - normalised_intrinsic(x, q), 0.0));
                q = -q;
            }
            // Map puts to calls
            if (q < 0.0)
            {
                x = -x;
            }
            if (beta <= 0.0) // For negative or zero prices we return 0.
                return 0.0;
            if (beta < Denormalization_Cutoff)
                // For positive but denormalized (a.k.a. 'subnormal') prices, we return 0 since it would be impossible to converge to full machine accuracy anyway.
                return 0.0;
            double b_max = Math.Exp(0.5 * x);
            if (beta >= b_max)
                return VolatilityValueToSignalPriceIsAboveMaximum;
            int iterations = 0, direction_reversal_count = 0;
            double f = -double.MaxValue, s = -double.MaxValue, ds = s, ds_previous = 0.0, s_left = double.Epsilon, s_right = double.MaxValue;
            // The temptation is great to use the optimised form b_c = exp(x/2)/2-exp(-x/2)·Phi(sqrt(-2·x)) but that would require implementing all of the above types of round-off and over/underflow handling for this expression, too.
            double s_c = Math.Sqrt(Math.Abs(2.0 * x)), b_c = NormalisedBlackCall(x, s_c), v_c = NormalisedVega(x, s_c);
            // Four branches.
            if (beta < b_c)
            {
                double s_l = s_c - b_c / v_c, b_l = NormalisedBlackCall(x, s_l);
                if (beta < b_l)
                {
                    double f_lower_map_l, d_f_lower_map_l_d_beta, d2_f_lower_map_l_d_beta2;
                    compute_f_lower_map_and_first_two_derivatives(x, s_l, out f_lower_map_l, out d_f_lower_map_l_d_beta, out d2_f_lower_map_l_d_beta2);
                    double r_ll =
                        RationalCubic.convex_rational_cubic_control_parameter_to_fit_second_derivative_at_right_side(0.0, b_l, 0.0, f_lower_map_l, 1.0,
                            d_f_lower_map_l_d_beta, d2_f_lower_map_l_d_beta2, true);
                    f = RationalCubic.rational_cubic_interpolation(beta, 0.0, b_l, 0.0, f_lower_map_l, 1.0, d_f_lower_map_l_d_beta, r_ll);
                    if (!(f > 0))
                    {
                        // This can happen due to roundoff truncation for extreme values such as |x|>500.
                        // We switch to quadratic interpolation using f(0)≡0, f(b_l), and f'(0)≡1 to specify the quadratic.
                        double t = beta / b_l;
                        f = (f_lower_map_l * t + b_l * (1 - t)) * t;
                    }
                    s = inverse_f_lower_map(x, f);
                    s_right = s_l;
                    //
                    // In this branch, which comprises the lowest segment, the objective function is
                    //     g(s) = 1/ln(b(x,s)) - 1/ln(beta)
                    //          ≡ 1/ln(b(s)) - 1/ln(beta)
                    // This makes
                    //              g'               =   -b'/(b·ln(b)²)
                    //              newton = -g/g'   =   (ln(beta)-ln(b))·ln(b)/ln(beta)·b/b'
                    //              halley = g''/g'  =   b''/b'  -  b'/b·(1+2/ln(b))
                    //              hh3    = g'''/g' =   b'''/b' +  2(b'/b)²·(1+3/ln(b)·(1+1/ln(b)))  -  3(b''/b)·(1+2/ln(b))
                    //
                    // The Householder(3) iteration is
                    //     s_n+1  =  s_n  +  newton · [ 1 + halley·newton/2 ] / [ 1 + newton·( halley + hh3·newton/6 ) ]
                    //
                    for (; iterations < n && Math.Abs(ds) > DoubleUtils.MachineEpsilon * s; ++iterations)
                    {
                        if (ds * ds_previous < 0)
                            ++direction_reversal_count;
                        if (iterations > 0 && (3 == direction_reversal_count || !(s > s_left && s < s_right)))
                        {
                            // If looping inefficently, or the forecast step takes us outside the bracket, or onto its edges, switch to binary nesting.
                            // NOTE that this can only really happen for very extreme values of |x|, such as |x| = |ln(F/K)| > 500.
                            s = 0.5 * (s_left + s_right);
                            if (s_right - s_left <= DoubleUtils.MachineEpsilon * s) break;
                            direction_reversal_count = 0;
                            ds = 0;
                        }
                        ds_previous = ds;
                        double b = NormalisedBlackCall(x, s), bp = NormalisedVega(x, s);
                        if (b > beta && s < s_right) s_right = s;
                        else if (b < beta && s > s_left) s_left = s; // Tighten the bracket if applicable.
                        if (b <= 0 || bp <= 0) // Numerical underflow. Switch to binary nesting for this iteration.
                            ds = 0.5 * (s_left + s_right) - s;
                        else
                        {
                            double ln_b = Math.Log(b),
                                ln_beta = Math.Log(beta),
                                bpob = bp / b,
                                h = x / s,
                                b_halley = h * h / s - s / 4,
                                newton = (ln_beta - ln_b) * ln_b / ln_beta / bpob,
                                halley = b_halley - bpob * (1 + 2 / ln_b);
                            double b_hh3 = b_halley * b_halley - 3 * (h * h / (s * s)) - 0.25,
                                hh3 = b_hh3 + 2 * (bpob * bpob) * (1 + 3 / ln_b * (1 + 1 / ln_b)) - 3 * b_halley * bpob * (1 + 2 / ln_b);
                            ds = newton * householder_factor(newton, halley, hh3);
                        }
                        s += ds = Math.Max(-0.5 * s, ds);
                    }
                    return s; //implied_volatility_output(iterations, s);
                }

                double v_l = NormalisedVega(x, s_l),
                    r_lm =
                        RationalCubic.convex_rational_cubic_control_parameter_to_fit_second_derivative_at_right_side(b_l, b_c, s_l, s_c, 1 / v_l,
                            1 / v_c, 0.0, false);
                s = RationalCubic.rational_cubic_interpolation(beta, b_l, b_c, s_l, s_c, 1 / v_l, 1 / v_c, r_lm);
                s_left = s_l;
                s_right = s_c;
            }
            else
            {
                double s_h = v_c > double.Epsilon ? s_c + (b_max - b_c) / v_c : s_c, b_h = NormalisedBlackCall(x, s_h);
                if (beta <= b_h)
                {
                    double v_h = NormalisedVega(x, s_h),
                        r_hm =
                            RationalCubic.convex_rational_cubic_control_parameter_to_fit_second_derivative_at_left_side(b_c, b_h, s_c, s_h, 1 / v_c,
                                1 / v_h, 0.0,
                                false);
                    s = RationalCubic.rational_cubic_interpolation(beta, b_c, b_h, s_c, s_h, 1 / v_c, 1 / v_h, r_hm);
                    s_left = s_c;
                    s_right = s_h;
                }
                else
                {
                    double f_upper_map_h, d_f_upper_map_h_d_beta, d2_f_upper_map_h_d_beta2;
                    compute_f_upper_map_and_first_two_derivatives(x, s_h, out f_upper_map_h, out d_f_upper_map_h_d_beta, out d2_f_upper_map_h_d_beta2);
                    if (d2_f_upper_map_h_d_beta2 > -SqrtDblMax && d2_f_upper_map_h_d_beta2 < SqrtDblMax)
                    {
                        double r_hh =
                            RationalCubic.convex_rational_cubic_control_parameter_to_fit_second_derivative_at_left_side(b_h, b_max, f_upper_map_h, 0.0,
                                d_f_upper_map_h_d_beta, -0.5, d2_f_upper_map_h_d_beta2, true);
                        f = RationalCubic.rational_cubic_interpolation(beta, b_h, b_max, f_upper_map_h, 0.0, d_f_upper_map_h_d_beta, -0.5, r_hh);
                    }
                    if (f <= 0.0)
                    {
                        double h = b_max - b_h, t = (beta - b_h) / h;
                        f = (f_upper_map_h * (1 - t) + 0.5 * h * t) * (1 - t);
                        // We switch to quadratic interpolation using f(b_h), f(b_max)≡0, and f'(b_max)≡-1/2 to specify the quadratic.
                    }
                    s = -2.0 * NormalDistribution.CumulativeInverse(f); //inverse_f_upper_map(f);
                    s_left = s_h;
                    if (beta > 0.5 * b_max)
                    {
                        // Else we better drop through and let the objective function be g(s) = b(x,s)-beta. 
                        //
                        // In this branch, which comprises the upper segment, the objective function is
                        //     g(s) = ln(b_max-beta)-ln(b_max-b(x,s))
                        //          ≡ ln((b_max-beta)/(b_max-b(s)))
                        // This makes
                        //              g'               =   b'/(b_max-b)
                        //              newton = -g/g'   =   ln((b_max-b)/(b_max-beta))·(b_max-b)/b'
                        //              halley = g''/g'  =   b''/b'  +  b'/(b_max-b)
                        //              hh3    = g'''/g' =   b'''/b' +  g'·(2g'+3b''/b')
                        // and the iteration is
                        //     s_n+1  =  s_n  +  newton · [ 1 + halley·newton/2 ] / [ 1 + newton·( halley + hh3·newton/6 ) ].
                        for (; iterations < n && Math.Abs(ds) > DoubleUtils.MachineEpsilon * s; ++iterations)
                        {
                            if (ds * ds_previous < 0)
                                ++direction_reversal_count;
                            if (iterations > 0 && (3 == direction_reversal_count || !(s > s_left && s < s_right)))
                            {
                                // If looping inefficently, or the forecast step takes us outside the bracket, or onto its edges, switch to binary nesting.
                                // NOTE that this can only really happen for very extreme values of |x|, such as |x| = |ln(F/K)| > 500.
                                s = 0.5 * (s_left + s_right);
                                if (s_right - s_left <= DoubleUtils.MachineEpsilon * s) break;
                                direction_reversal_count = 0;
                                ds = 0.0;
                            }
                            ds_previous = ds;
                            double b = NormalisedBlackCall(x, s), bp = NormalisedVega(x, s);
                            if (b > beta && s < s_right) s_right = s;
                            else if (b < beta && s > s_left) s_left = s; // Tighten the bracket if applicable.
                            if (b >= b_max || bp <= double.Epsilon) // Numerical underflow. Switch to binary nesting for this iteration.
                                ds = 0.5 * (s_left + s_right) - s;
                            else
                            {
                                double b_max_minus_b = b_max - b, g = Math.Log((b_max - beta) / b_max_minus_b), gp = bp / b_max_minus_b;
                                double b_halley = (x * x / (s * s)) / s - s / 4, b_hh3 = b_halley * b_halley - 3 * (x * x / (s * s * s * s)) - 0.25;
                                double newton = -g / gp, halley = b_halley + gp, hh3 = b_hh3 + gp * (2 * gp + 3 * b_halley);
                                ds = newton * householder_factor(newton, halley, hh3);
                            }
                            s += ds = Math.Max(-0.5 * s, ds);
                        }
                        return s; //implied_volatility_output(iterations, s);
                    }
                }
            }
            // In this branch, which comprises the two middle segments, the objective function is g(s) = b(x,s)-beta, or g(s) = b(s) - beta, for short.
            // This makes
            //              newton = -g/g'   =  -(b-beta)/b'
            //              halley = g''/g'  =    b''/b'    =  x²/s³-s/4
            //              hh3    = g'''/g' =    b'''/b'   =  halley² - 3·(x/s²)² - 1/4
            // and the iteration is
            //     s_n+1  =  s_n  +  newton · [ 1 + halley·newton/2 ] / [ 1 + newton·( halley + hh3·newton/6 ) ].
            //
            for (; iterations < n && Math.Abs(ds) > DoubleUtils.MachineEpsilon * s; ++iterations)
            {
                if (ds * ds_previous < 0)
                    ++direction_reversal_count;
                if (iterations > 0 && (3 == direction_reversal_count || !(s > s_left && s < s_right)))
                {
                    // If looping inefficently, or the forecast step takes us outside the bracket, or onto its edges, switch to binary nesting.
                    // NOTE that this can only really happen for very extreme values of |x|, such as |x| = |ln(F/K)| > 500.
                    s = 0.5 * (s_left + s_right);
                    if (s_right - s_left <= DoubleUtils.MachineEpsilon * s) break;
                    direction_reversal_count = 0;
                    ds = 0.0;
                }
                ds_previous = ds;
                double b = NormalisedBlackCall(x, s), bp = NormalisedVega(x, s);
                if (b > beta && s < s_right) s_right = s;
                else if (b < beta && s > s_left) s_left = s; // Tighten the bracket if applicable.
                double newton = (beta - b) / bp, halley = (x * x / (s * s)) / s - s / 4.0, hh3 = halley * halley - 3.0 * (x * x / (s * s * s * s)) - 0.25;
                s += ds = Math.Max(-0.5 * s, newton * householder_factor(newton, halley, hh3));
            }
            return s; //implied_volatility_output(iterations, s);
        }

        public const double VolatilityValueToSignalPriceIsBelowIntrinsic = -double.MaxValue;
        public const double VolatilityValueToSignalPriceIsAboveMaximum = double.MaxValue;
    }
}