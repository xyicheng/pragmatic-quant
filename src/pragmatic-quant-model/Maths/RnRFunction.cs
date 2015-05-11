using System;
using System.Diagnostics.Contracts;
using System.Linq;

namespace pragmatic_quant_model.Maths
{
    public abstract class RnRFunction
    {
        protected RnRFunction(int dim)
        {
            Dim = dim;
        }

        public abstract double Eval(double[] x);
        public int Dim { get; private set; }
        public virtual RnRFunction Mult(RnRFunction right)
        {
            return new ProductRnRFunction(this, right);
        }

        public static RnRFunction operator *(RnRFunction f, RnRFunction g)
        {
            return f.Mult(g);
        }
    }

    public static class RnRFunctions
    {
        public static RnRFunction Constant(double value, int dim)
        {
            return new ConstantRnRFunction(dim, value);
        }
        public static RnRFunction ExpAffine(double weight, double[] mults)
        {
            return new ExpAffineRnRFunction(weight, mults);
        }
    }

    public class ConstantRnRFunction : RnRFunction
    {
        public ConstantRnRFunction(int dim, double value) : base(dim)
        {
            Value = value;
        }
        public override double Eval(double[] x)
        {
            return Value;
        }
        public double Value { get; private set; }
    }

    public class ExpAffineRnRFunction : RnRFunction
    {
        #region private fields
        private readonly double weight;
        private readonly double[] mults;
        #endregion
        public ExpAffineRnRFunction(double weight, double[] mults)
            : base(mults.Length)
        {
            this.weight = weight;
            this.mults = mults;
        }
        public override double Eval(double[] x)
        {
            return weight * Math.Exp(mults.DotProduct(x));
        }
        public override RnRFunction Mult(RnRFunction right)
        {
            var cst = right as ConstantRnRFunction;
            if (cst != null)
            {
                return new ExpAffineRnRFunction(cst.Value * weight, mults);
            }

            var expAff = right as ExpAffineRnRFunction;
            if (expAff != null)
            {
                return new ExpAffineRnRFunction(expAff.weight * weight, expAff.mults.Add(mults));
            }

            return base.Mult(right);
        }
    }

    public class ProductRnRFunction : RnRFunction
    {
        #region private fields
        private readonly RnRFunction[] terms;
        #endregion
        public ProductRnRFunction(params RnRFunction[] terms)
            : base(terms.First().Dim)
        {
            Contract.Requires(terms.Select(f => f.Dim).Distinct().Count() == 1);
            this.terms = terms;
        }
        public override double Eval(double[] x)
        {
            return terms.Aggregate(1.0, (prev, f) => prev * f.Eval(x));
        }
    }
}