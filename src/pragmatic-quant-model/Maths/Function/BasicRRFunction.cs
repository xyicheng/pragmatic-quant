using System;
using System.Linq;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.Maths.Function
{
    public class ConstantRrFunction : RrFunction
    {
        public ConstantRrFunction(double value)
        {
            Value = value;
        }
        public readonly double Value;
        public override double Eval(double x)
        {
            return Value;
        }
    }


    public class ExpRrFunction : RrFunction
    {
        #region private fields
        private readonly double weight;
        private readonly double slope;
        #endregion
        private ExpRrFunction(double weight, double slope)
        {
            this.weight = weight;
            this.slope = slope;
        }

        public static RrFunction Create(double weight, double slope)
        {
            if (DoubleUtils.EqualZero(weight))
                    return RrFunctions.Zero;
            
            if (DoubleUtils.EqualZero(slope))
                return RrFunctions.Constant(weight);

            return new ExpRrFunction(weight, slope);
        }
        public override double Eval(double x)
        {
            return weight * Math.Exp(slope * x);
        }
        public override RrFunction Mult(RrFunction other)
        {
            var cst = other as ConstantRrFunction;
            if (cst != null)
            {
                return Create(weight * cst.Value, slope);
            }

            var exp = other as ExpRrFunction;
            if (exp != null)
            {
                return Create(weight * exp.weight, slope + exp.slope);
            }

            return base.Mult(other);
        }
        public override RrFunction Integral(double basePoint)
        {
            if (DoubleUtils.EqualZero(slope))
                return RrFunctions.Constant(weight).Integral(basePoint);

            var zeroBaseIntegral = Create(weight / slope, slope);
            return zeroBaseIntegral - zeroBaseIntegral.Eval(basePoint);
        }
    }

    public class FuncRrFunction : RrFunction
    {
        #region private fields
        private readonly Func<double, double> f;
        #endregion
        public FuncRrFunction(Func<double, double> f)
        {
            this.f = f;
        }
        public override double Eval(double x)
        {
            return f(x);
        }
    }

    public class LinearCombinationRrFunction : RrFunction
    {
        #region private fields
        private readonly double[] weights;
        private readonly RrFunction[] functions;
        #endregion
        public LinearCombinationRrFunction(double[] weights, RrFunction[] functions)
        {
            if (weights.Length != functions.Length)
                throw new Exception("LinearCombinationRRFunction : incompatible size input");
            this.weights = weights;
            this.functions = functions;
        }
        public override double Eval(double x)
        {
            return weights.Select((w, i) => w * functions[i].Eval(x)).Sum();
        }
    }

    public class ProductRrFunction : RrFunction
    {
        #region private fields
        private readonly double weight;
        private readonly RrFunction[] prods;
        #endregion
        public ProductRrFunction(double weight, params RrFunction[] prods)
        {
            this.weight = weight;
            this.prods = prods;
        }
        public override double Eval(double x)
        {
            return prods.Aggregate(weight, (current, t) => current * t.Eval(x));
        }
    }
}
