using System;
using System.Linq;

namespace pragmatic_quant_model.Math.Function
{
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
}
