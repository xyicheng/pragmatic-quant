using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths.Interpolation;

namespace pragmatic_quant_model.Maths.Function
{
    [DebuggerDisplay("ConstantRrFunction Value={Value}")]
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
        public override RrFunction Mult(RrFunction other)
        {
            var cst = other as ConstantRrFunction;
            if (cst != null)
                return new ConstantRrFunction(Value * cst.Value);

            var exp = other as ExpRrFunction;
            if (exp != null)
                return ExpRrFunction.Mult(exp, this);

            var step = other as StepFunction;
            if (step != null)
                return StepFunction.Mult(step, this);

            var lc = other as LinearCombinationRrFunction;
            if (lc != null)
                return LinearCombinationRrFunction.Mult(lc, this);

            return base.Mult(other);
        }
        public override RrFunction Integral(double basePoint)
        {
            return new LinearInterpolation(new[] {basePoint}, new[] {0.0}, Value, Value);
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
                return Mult(this, cst);

            var exp = other as ExpRrFunction;
            if (exp != null)
                return Mult(this, exp);
            
            return base.Mult(other);
        }
        public static RrFunction Mult(ExpRrFunction exp, ConstantRrFunction cst)
        {
            return Create(exp.weight * cst.Value, exp.slope);
        }
        public static RrFunction Mult(ExpRrFunction letfExp, ExpRrFunction rightExp)
        {
            return Create(letfExp.weight * rightExp.weight, letfExp.slope + rightExp.slope);
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

        public LinearCombinationRrFunction(double[] weights, RrFunction[] functions)
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

        public override double Eval(double x)
        {
            return weights.Select((w, i) => w * functions[i].Eval(x)).Sum();
        }

        public override RrFunction Mult(RrFunction other)
        {
            var cst = other as ConstantRrFunction;
            if (cst != null)
                return Mult(this, cst);

            return Create(weights, functions.Map(f => f.Mult(other)));
        }
        public static RrFunction Mult(LinearCombinationRrFunction lc, ConstantRrFunction cst)
        {
            if (DoubleUtils.MachineEquality(1.0, cst.Value))
                return lc;

            if (DoubleUtils.EqualZero(cst.Value))
                return RrFunctions.Zero;

            return Create(lc.weights.Map(w => w * cst.Value), lc.functions);
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
