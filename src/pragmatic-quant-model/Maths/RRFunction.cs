using System;
using System.Linq;
using pragmatic_quant_model.Maths.Function;
using pragmatic_quant_model.Maths.Interpolation;

namespace pragmatic_quant_model.Maths
{
    public abstract class RrFunction
    {
        public abstract double Eval(double x);

        public virtual RrFunction Integral(double basePoint)
        {
            throw new NotImplementedException("RrFunction.Integral");
        }
        public virtual RrFunction Add(RrFunction other)
        {
            return RrFunctions.Sum(this, other);
        }
        public virtual RrFunction Mult(RrFunction other)
        {
            return RrFunctions.Product(this, other);
        }

        public static RrFunction operator +(RrFunction f, RrFunction g)
        {
            return f.Add(g);
        }
        public static RrFunction operator +(RrFunction f, double a)
        {
            return f.Add(RrFunctions.Constant(a));
        }
        public static RrFunction operator +(double a, RrFunction f)
        {
            return f.Add(RrFunctions.Constant(a));
        }
        
        public static RrFunction operator -(RrFunction f, double a)
        {
            return f.Add(RrFunctions.Constant(-a));
        }
        
        public static RrFunction operator *(RrFunction f, RrFunction g)
        {
            return f.Mult(g);
        }
        public static RrFunction operator *(RrFunction f, double a)
        {
            return f.Mult(RrFunctions.Constant(a));
        }
        public static RrFunction operator *(double a, RrFunction f)
        {
            return f.Mult(RrFunctions.Constant(a));
        }
    }

    public static class RrFunctions
    {
        public static readonly RrFunction Zero = Constant(0.0);
        public static RrFunction Constant(double value)
        {
            return new ConstantRrFunction(value);
        }
        public static RrFunction Exp(double slope)
        {
            return ExpRrFunction.Create(1.0, slope);
        }
        public static RrFunction Func(Func<double, double> f)
        {
            return new FuncRrFunction(f);
        }
        public static RrFunction Sum(params RrFunction[] functions)
        {
            return new LinearCombinationRrFunction(functions.Select(f => 1.0).ToArray(), functions);
        }
        public static RrFunction LinearCombination(double[] weights, RrFunction[] funcs)
        {
            return new LinearCombinationRrFunction(weights, funcs);
        }
        public static RrFunction Product(RrFunction f, RrFunction g)
        {
            return new ProductRrFunction(1.0, f, g);
        }

        public static RrFunction LinearInterpolation(double[] abscissae, double[] values,
                                                    double leftExtrapolationSlope, double rightExtrapolationSlope)
        {
            return new LinearInterpolation(abscissae, values, leftExtrapolationSlope, rightExtrapolationSlope);
        }
    }
}
