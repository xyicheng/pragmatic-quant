using System;
using System.Linq;
using pragmatic_quant_model.Math.Function;

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

        public static RRFunction LinearInterpolation(double[] abscissae, double[] values,
                                                    double leftExtrapolationSlope, double rightExtrapolationSlope)
        {
            return new LinearInterpolation(abscissae, values, leftExtrapolationSlope, rightExtrapolationSlope);
        }
    }
}
