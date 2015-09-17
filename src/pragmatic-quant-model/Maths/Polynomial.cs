using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.Maths
{
    [DebuggerDisplay("{ToString()}")]
    public class Polynomial
    {
        public Polynomial(params double[] coeffs)
            : this(false, coeffs) { }
        public Polynomial(bool simplify, params double[] coeffs)
        {
            Contract.Requires(coeffs.Length > 0);

            if (simplify)
            {
                int degree = coeffs.Length - 1;
                while (degree > 0)
                {
                    if (!DoubleUtils.EqualZero(coeffs[degree]))
                        break;
                    --degree;
                }
                Coeffs = new double[degree + 1];
                Array.Copy(coeffs, Coeffs, Coeffs.Length);
            }
            else
            {
                Coeffs = coeffs;
            }
        }
        public readonly double[] Coeffs;
        public int Degree { get { return Coeffs.Length - 1; } }
        public override string ToString()
        {
            var firstNonZeroCoeff = new List<double>(Coeffs).FindIndex(x => !DoubleUtils.EqualZero(x));

            string desc = "";
            bool initialized = false;

            //Display constant part
            if (firstNonZeroCoeff == 0)
            {
                desc += Coeffs[0].ToString(CultureInfo.InvariantCulture);
                initialized = true;
            }

            //Display degree one part
            if (firstNonZeroCoeff <= 1 && Coeffs.Length > 1)
            {
                if (initialized)
                    desc += " + ";

                if (DoubleUtils.MachineEquality(Coeffs[1], 1.0))
                    desc += "X";
                else
                    desc += string.Format("{0} * X", Coeffs[1]);

                initialized = true;
            }
                
            //Display higher degrees
            for (int i = 2; i < Coeffs.Length; i++)
            {
                if (!DoubleUtils.EqualZero(Coeffs[i]))
                {
                    if (initialized)
                        desc += " + ";

                    if (DoubleUtils.MachineEquality(Coeffs[i], 1.0))
                        desc += string.Format("X^{0}", i);
                    else
                        desc += string.Format("{0} * X^{1}", Coeffs[i], i);

                    initialized = true;
                }
            }
            return desc;
        }

        public static readonly Polynomial X = new Polynomial(0.0, 1.0);
        public static readonly Polynomial Zero = new Polynomial(0.0);

        public static implicit operator Polynomial(double a)
        {
            return new Polynomial(a);
        }
        public static Polynomial operator +(Polynomial left, Polynomial right)
        {
            return PolynomialUtils.Add(left, right);
        }
        public static Polynomial operator -(Polynomial left, Polynomial right)
        {
            return PolynomialUtils.Sub(left, right);
        }
        public static Polynomial operator -(Polynomial p)
        {
            return PolynomialUtils.Sub(0.0, p);
        }
        public static Polynomial operator *(Polynomial left, Polynomial right)
        {
            return PolynomialUtils.Mult(left, right);
        }
        public static RationalFraction operator /(Polynomial num, Polynomial  denom )
        {
            return num / (RationalFraction) denom;
        }
    }

    public static class PolynomialUtils
    {
        public static Polynomial Add(Polynomial left, Polynomial right)
        {
            double[] rightCoeffs = right.Coeffs;
            double[] leftCoeffs = left.Coeffs;

            var result = new double[Math.Max(leftCoeffs.Length, rightCoeffs.Length)];
            leftCoeffs.CopyTo(result, 0);
            for (int i = 0; i < rightCoeffs.Length; i++)
                result[i] += rightCoeffs[i];

            return new Polynomial(true, result);
        }
        public static Polynomial Sub(Polynomial left, Polynomial right)
        {
            var maxDegree = Math.Max(left.Coeffs.Length, right.Coeffs.Length);
            var result = new double[maxDegree];
            left.Coeffs.CopyTo(result, 0);
            for (int i = 0; i < right.Coeffs.Length; i++)
                result[i] -= right.Coeffs[i];
            return new Polynomial(true, result);
        }
        public static Polynomial Mult(Polynomial left, Polynomial right)
        {
            var leftCoeffs = left.Coeffs;
            var rightCoeffs = right.Coeffs;
            var maxDegree = leftCoeffs.Length + rightCoeffs.Length - 1;
            var result = new double[maxDegree];
            for (int i = 0; i < leftCoeffs.Length; i++)
            {
                var ithLeftCoeffs = leftCoeffs[i];
                for (int j = 0; j < rightCoeffs.Length; j++)
                {
                    result[i + j] += ithLeftCoeffs * rightCoeffs[j];
                }

            }
            return new Polynomial(result);
        }
        
        public static double Eval(this Polynomial p, double x)
        {
            var coeffs = p.Coeffs;
            var result = coeffs[coeffs.Length - 1];
            for (int d = coeffs.Length - 2; d >= 0; d--)
            {
                result = coeffs[d] + x * result;
            }
            return result;
        }
        public static Polynomial Derivative(this Polynomial p)
        {
            if (p.Degree == 0)
                return Polynomial.Zero;

            double[] c = p.Coeffs;
            var derivCoeffs = new double[c.Length - 1];
            for (int i = 0; i <derivCoeffs.Length; ++i)
            {
                derivCoeffs[i] = (i + 1) * c[i + 1];
            }
            return new Polynomial(derivCoeffs);
        }
        public static Polynomial TaylorDev(this Polynomial p, double x)
        {
            double[] c = p.Coeffs;
            var taylor = new double[c.Length];
            int nc = c.Length - 1, nd = taylor.Length - 1;

            taylor[0] = c[nc];
            for (int j = 1; j < nd + 1; j++)
                taylor[j] = 0.0;

            for (int i = nc - 1; i >= 0; i--)
            {
                int nnd = (nd < (nc - i) ? nd : nc - i);
                for (int j = nnd; j > 0; j--)
                    taylor[j] = taylor[j] * x + taylor[j - 1];
                taylor[0] = taylor[0] * x + c[i];
            }
            return new Polynomial(taylor);
        }
        
        public static bool IsZero(this Polynomial p)
        {
            return p.Coeffs.All(DoubleUtils.EqualZero);
        }
        public static bool IsUnity(this Polynomial p)
        {
            return DoubleUtils.MachineEquality(p.Coeffs[0], 1.0) &&
                   p.Coeffs.Skip(1).All(DoubleUtils.EqualZero);
        }
    }

    [DebuggerDisplay("{ToString()}")]
    public class RationalFraction
    {
        public RationalFraction(Polynomial num, Polynomial denom)
        {
            Contract.Requires(num.Coeffs.Length > 0);
            Contract.Requires(denom.Coeffs.Length > 0);
            Num = num;
            Denom = denom;
        }
        public readonly Polynomial Num;
        public readonly Polynomial Denom;
        public override string ToString()
        {
            if (Denom.IsUnity())
                return Num.ToString();

            return string.Format("({0})/({1})", Num, Denom);
        }

        public static implicit operator RationalFraction(double a)
        {
            return new RationalFraction(a, 1.0);
        }
        public static implicit operator RationalFraction(Polynomial p)
        {
            return new RationalFraction(p, 1.0);
        }
        public static RationalFraction operator *(RationalFraction left, RationalFraction right)
        {
            return new RationalFraction(left.Num * right.Num, left.Denom * right.Denom);
        }
        public static RationalFraction operator /(RationalFraction num, RationalFraction denom)
        {
            return new RationalFraction(num.Num * denom.Denom, num.Denom * denom.Num);
        }
        public static RationalFraction operator +(RationalFraction left, RationalFraction right)
        {
            return (left.Num * right.Denom + right.Num * left.Denom) / (RationalFraction)(left.Denom * right.Denom);
        }
    }

    public static class RationalFractionUtils
    {
        public static double Eval(this RationalFraction f, double x)
        {
            return f.Num.Eval(x) / f.Denom.Eval(x);
        }
        public static RationalFraction Derivative(this RationalFraction f)
        {
            return (f.Num.Derivative() * f.Denom - f.Num * f.Denom.Derivative()) / (RationalFraction)(f.Denom * f.Denom);
        }
        public static Polynomial AsPolynomial(this RationalFraction f)
        {
            if (f.Denom.Degree > 0)
                throw new Exception(String.Format("{0} cannot be converted to polynomial !", f));
            return f.Num * (1.0 / f.Denom.Coeffs[0]);
        }
    }
}