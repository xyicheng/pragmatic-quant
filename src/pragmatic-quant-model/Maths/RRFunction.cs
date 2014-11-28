using System;
using System.Linq;
using pragmatic_quant_model.Maths.Function;

namespace pragmatic_quant_model.Maths
{
    public abstract class RRFunction
    {
        public abstract double Eval(double x);
        public virtual RRFunction Add(RRFunction other)
        {
            return RRFunctions.Sum(this, other);
        }
        public virtual RRFunction Mult(RRFunction other)
        {
            return RRFunctions.Product(this, other);
        }

        public static RRFunction operator +(RRFunction f, RRFunction g)
        {
            return f.Add(g);
        }
        public static RRFunction operator *(RRFunction f, RRFunction g)
        {
            return f.Mult(g);
        }
    }

    public static class RRFunctions
    {
        public static RRFunction Constant(double value)
        {
            return new ConstantRRFunction(value);
        }
        public static RRFunction Exp(double slope)
        {
            return new ExpRRFunction(1.0, slope);
        }
        public static RRFunction Func(Func<double, double> f)
        {
            return new FuncRRFunction(f);
        }
        public static RRFunction Sum(params RRFunction[] functions)
        {
            return new LinearCombinationRRFunction(functions.Select(f => 1.0).ToArray(), functions);
        }
        public static RRFunction LinearCombination(double[] weights, RRFunction[] funcs)
        {
            return new LinearCombinationRRFunction(weights, funcs);
        }
        public static RRFunction Product(RRFunction f, RRFunction g)
        {
            return new ProductRRFunction(1.0, f, g);
        }

        public static RRFunction LinearInterpolation(double[] abscissae, double[] values,
                                                    double leftExtrapolationSlope, double rightExtrapolationSlope)
        {
            return new LinearInterpolation(abscissae, values, leftExtrapolationSlope, rightExtrapolationSlope);
        }
    }
}
