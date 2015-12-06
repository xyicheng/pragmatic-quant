using System;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths.Interpolation;

namespace pragmatic_quant_model.Maths.Function
{
    public abstract class RrFunction
    {
        public abstract double Eval(double x);

        public virtual RrFunction Derivative()
        {
            throw new NotImplementedException("RrFunction.Derivative");
        }
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
            var step = other as StepFunction;
            if (step != null)
                return step.Mult(this);

            return RrFunctions.Product(this, other);
        }
        public virtual RrFunction Inverse()
        {
            throw new NotImplementedException("RrFunction.Inverse");
        }
        
        public static implicit operator RrFunction(double cst)
        {
            return RrFunctions.Constant(cst);
        }
        public static RrFunction operator +(RrFunction f, RrFunction g)
        {
            return f.Add(g);
        }
        public static RrFunction operator -(RrFunction f, RrFunction g)
        {
            return f.Add(g.Mult(-1.0));
        }
        public static RrFunction operator *(RrFunction f, RrFunction g)
        {
            return f.Mult(g);
        }
        public static RrFunction operator /(RrFunction f, RrFunction g)
        {
            return f * g.Inverse();
        }
    }

    public static class RrFunctions
    {
        public static readonly RrFunction Zero = new ConstantRrFunction(0.0);
        public static RrFunction Constant(double value)
        {
            if (DoubleUtils.EqualZero(value))
                return Zero;
            return new ConstantRrFunction(value);
        }
        public static RrFunction Exp(double slope)
        {
            return ExpRrFunction.Create(1.0, slope);
        }
        public static RrFunction Affine(double slope, double origin)
        {
            if (DoubleUtils.EqualZero(slope))
                return Constant(origin);
            return Constant(slope).Integral(-origin / slope);
        }
        public static RrFunction Func(Func<double, double> f)
        {
            return new FuncRrFunction(f);
        }
        public static RrFunction Sum(params RrFunction[] functions)
        {
            return LinearCombinationRrFunction.Create(functions.Select(f => 1.0).ToArray(), functions);
        }
        public static RrFunction LinearCombination(double[] weights, RrFunction[] funcs)
        {
            return LinearCombinationRrFunction.Create(weights, funcs);
        }
        public static RrFunction Product(RrFunction f, RrFunction g)
        {
            return new ProductRrFunction(1.0, f, g);
        }

        public static RrFunction LinearInterpolation(double[] abscissae, double[] values,
            double leftExtrapolationSlope = 0.0, double rightExtrapolationSlope = 0.0)
        {
            return SplineInterpoler.BuildLinearSpline(abscissae, values, leftExtrapolationSlope, rightExtrapolationSlope);
        }
    }
}
