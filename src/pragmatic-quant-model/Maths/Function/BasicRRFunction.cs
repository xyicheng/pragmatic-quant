using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths.Interpolation;

namespace pragmatic_quant_model.Maths.Function
{
    [DebuggerDisplay("Constant Value = {Value}")]
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
        public override RrFunction Add(RrFunction other)
        {
            var cst = other as ConstantRrFunction;
            if (cst != null)
                return RrFunctions.Constant(Value + cst.Value);

            var step = other as StepFunction;
            if (step != null)
                return StepFunction.Add(step, this);

            var spline = other as SplineInterpoler;
            if (spline != null)
                return SplineInterpoler.Add(spline, this);

            return base.Add(other);
        }
        public override RrFunction Mult(RrFunction other)
        {
            var cst = other as ConstantRrFunction;
            if (cst != null)
                return ProductRrFunctions.Mult(this, cst);

            var exp = other as ExpRrFunction;
            if (exp != null)
                return ProductRrFunctions.Mult(exp, this);

            var step = other as StepFunction;
            if (step != null)
                return StepFunction.Mult(step, this);

            var spline = other as SplineInterpoler;
            if (spline != null)
                return SplineInterpoler.Mult(spline, this);

            var lc = other as LinearCombinationRrFunction;
            if (lc != null)
                return ProductRrFunctions.Mult(lc, this);

            return base.Mult(other);
        }
        public override RrFunction Integral(double basePoint)
        {
            return RrFunctions.LinearInterpolation(new[] {basePoint}, new[] {0.0}, Value, Value);
        }
        public override RrFunction Derivative()
        {
            return RrFunctions.Zero;
        }
        public override RrFunction Inverse()
        {
            return RrFunctions.Constant(1.0 / Value);
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

        public double Weight
        {
            get { return weight; }
        }
        public double Slope
        {
            get { return slope; }
        }

        public override double Eval(double x)
        {
            return weight * Math.Exp(slope * x);
        }
        public override RrFunction Mult(RrFunction other)
        {
            var cst = other as ConstantRrFunction;
            if (cst != null)
                return ProductRrFunctions.Mult(this, cst);

            var exp = other as ExpRrFunction;
            if (exp != null)
                return ProductRrFunctions.Mult(this, exp);

            return base.Mult(other);
        }
        public override RrFunction Integral(double basePoint)
        {
            if (DoubleUtils.EqualZero(slope))
                return RrFunctions.Constant(weight).Integral(basePoint);

            var zeroBaseIntegral = Create(weight / slope, slope);
            return zeroBaseIntegral - zeroBaseIntegral.Eval(basePoint);
        }
        public override RrFunction Derivative()
        {
            return Create(weight * slope, slope);
        }
        public override RrFunction Inverse()
        {
            return Create(1.0 / weight, -slope);
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
        #region private methods
        private static void AddTerm(ref IList<double> weights, ref IList<RrFunction> functions,
            double weight, RrFunction term)
        {
            var termIndex = functions.IndexOf(term);
            if (termIndex > 0)
            {
                weights[termIndex] += weight;
                return;
            }
            weights.Add(weight);
            functions.Add(term);
        }
        private static void AddTerms(double[] weightTerms, RrFunction[] terms,
            ref IList<double> weights, ref IList<RrFunction> functions)
        {
            for (int i = 0; i < weightTerms.Length; i++)
                AddTerm(ref weights, ref functions, weightTerms[i], terms[i]);
        }
        #endregion

        private LinearCombinationRrFunction(double[] weights, RrFunction[] functions)
        {
            if (weights.Length != functions.Length)
                throw new Exception("LinearCombinationRRFunction : incompatible size input");
            this.weights = weights;
            this.functions = functions;
        }
        public static RrFunction Create(double[] weights, RrFunction[] functions)
        {
            IList<double> ws = new List<double>();
            IList<RrFunction> fs = new List<RrFunction>();
            AddTerms(weights, functions, ref ws, ref fs);
            return new LinearCombinationRrFunction(ws.ToArray(), fs.ToArray());
        }

        public double[] Weights
        {
            get { return weights; }
        }
        public RrFunction[] Functions
        {
            get { return functions; }
        }

        public override double Eval(double x)
        {
            return weights.Select((w, i) => w * functions[i].Eval(x)).Sum();
        }

        public override RrFunction Mult(RrFunction other)
        {
            var cst = other as ConstantRrFunction;
            if (cst != null)
                return ProductRrFunctions.Mult(this, cst);

            return Create(weights, functions.Map(f => f.Mult(other)));
        }
        
        public override RrFunction Add(RrFunction other)
        {
            var lc = other as LinearCombinationRrFunction;
            if (lc != null)
                return Add(this, lc);

            IList<double> ws = new List<double>(weights);
            IList<RrFunction> fs = new List<RrFunction>(functions);
            AddTerm(ref ws, ref fs, 1.0, other);
            return new LinearCombinationRrFunction(ws.ToArray(), fs.ToArray());
        }
        public static RrFunction Add(LinearCombinationRrFunction leftLc, LinearCombinationRrFunction rightLc)
        {
            IList<double> ws = new List<double>();
            IList<RrFunction> fs = new List<RrFunction>();
            AddTerms(leftLc.weights, leftLc.functions, ref ws, ref fs);
            AddTerms(rightLc.weights, rightLc.functions, ref ws, ref fs);
            return new LinearCombinationRrFunction(ws.ToArray(), fs.ToArray());
        }

        public override RrFunction Integral(double basePoint)
        {
            return Create(weights, functions.Map(f => f.Integral(basePoint)));
        }
        public override RrFunction Derivative()
        {
            return Create(weights, functions.Map(f => f.Derivative()));
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

    public static class ProductRrFunctions
    {
        public static RrFunction Mult(ConstantRrFunction left, ConstantRrFunction right)
        {
            return RrFunctions.Constant(left.Value * right.Value);
        }
        public static RrFunction Mult(ExpRrFunction exp, ConstantRrFunction cst)
        {
            return ExpRrFunction.Create(exp.Weight * cst.Value, exp.Slope);
        }
        public static RrFunction Mult(ExpRrFunction letfExp, ExpRrFunction rightExp)
        {
            return ExpRrFunction.Create(letfExp.Weight * rightExp.Weight, letfExp.Slope + rightExp.Slope);
        }
        public static RrFunction Mult(LinearCombinationRrFunction lc, ConstantRrFunction cst)
        {
            if (DoubleUtils.MachineEquality(1.0, cst.Value))
                return lc;

            if (DoubleUtils.EqualZero(cst.Value))
                return RrFunctions.Zero;

            return LinearCombinationRrFunction.Create(lc.Weights.Map(w => w * cst.Value), lc.Functions);
        }
    }

}
