using System;
using System.Linq;

namespace pragmatic_quant_model.Math
{
    public abstract class RRFunction
    {
        public abstract double Eval(double x);
        public virtual RRFunction Add(RRFunction other)
        {
            return RRFunctions.Sum(this, other);
        }
    }

    public static class RRFunctions
    {
        public static RRFunction Constant(double value)
        {
            return new ConstantRRFunction(value);
        }
        public static RRFunction Func(Func<double, double> f)
        {
            return new FuncRRFunction(f);
        }
        public static RRFunction Sum(params RRFunction[] functions)
        {
            return new LinearCombinationRRFunction(functions.Select(f => 1.0).ToArray(), functions);
        }
    }
    
    public class ConstantRRFunction : RRFunction
    {
        public ConstantRRFunction(double value)
        {
            Value = value;
        }
        public readonly double Value;
        public override double Eval(double x)
        {
            return Value;
        }
    }

    public class FuncRRFunction : RRFunction
    {
        #region private fields
        private readonly Func<double, double> f;
        #endregion
        public FuncRRFunction(Func<double, double> f)
        {
            this.f = f;
        }
        public override double Eval(double x)
        {
            return f(x);
        }
    }

    public class LinearCombinationRRFunction : RRFunction
    {
        #region private fields
        private readonly double[] weights;
        private readonly RRFunction[] functions;
        #endregion
        public LinearCombinationRRFunction(double[] weights, RRFunction[] functions)
        {
            if (weights.Length!=functions.Length)
                throw new Exception("LinearCombinationRRFunction : incompatible size input");
            this.weights = weights;
            this.functions = functions;
        }
        public override double Eval(double x)
        {
            return weights.Select((w, i) => w * functions[i].Eval(x)).Sum();
        }
    }
}
