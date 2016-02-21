using System;
using System.Linq;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.Maths.Function
{
    public class StepFunction : RrFunction
    {
        #region private fields
        private readonly StepFunction<double> evaluator; 
        private readonly double[] abscissae;
        private readonly double[] values;
        private readonly double leftValue;
        #endregion
        #region private methods
        private static RrFunction BinaryOp(StepFunction left, StepFunction right,
                                           Func<double, double, double> binaryOp)
        {
            var mergedAbscissae = left.abscissae.Union(right.abscissae).OrderBy(p => p).ToArray();
            var multValues = mergedAbscissae.Map(p => binaryOp(left.Eval(p), right.Eval(p)));
            var multLeftValue = binaryOp(left.Eval(double.NegativeInfinity), right.Eval(double.NegativeInfinity));
            return new StepFunction(mergedAbscissae, multValues, multLeftValue);
        }
        #endregion
        public StepFunction(StepFunction<double> steps)
        {
            evaluator = steps;
            abscissae = steps.Pillars;
            values = steps.Values;
            leftValue = steps.LeftValue;
        }
        public StepFunction(double[] abscissae, double[] values, double leftValue)
        {
            evaluator = new StepFunction<double>(abscissae, values, leftValue);
            this.abscissae = abscissae;
            this.values = values;
            this.leftValue = leftValue;
        }
        public override double Eval(double x)
        {
            return evaluator.Eval(x);
        }
        
        public override RrFunction Integral(double basePoint)
        {
            var zeroBaseIntegrals = Enumerable.Range(0, abscissae.Length)
                .Map(i => i == 0 ? 0.0 : values[i - 1] * (abscissae[i] - abscissae[i - 1]))
                .Scan(0.0, (sum, current) => sum + current);
            var zeroBasedIntegral = RrFunctions.LinearInterpolation(abscissae, zeroBaseIntegrals,
                leftValue, values[values.Length - 1]);
            return zeroBasedIntegral - zeroBasedIntegral.Eval(basePoint);
        }
        public override RrFunction Derivative()
        {
            throw new Exception("StepFunction derivative is not a function ");
        }
        public override RrFunction Inverse()
        {
            return new StepFunction(abscissae, values.Map(v => 1.0 / v), 1.0 / leftValue);
        }

        public override RrFunction Add(RrFunction other)
        {
            var cst = other as ConstantRrFunction;
            if (cst != null)
                return Add(this, cst);

            var step = other as StepFunction;
            if (step != null)
                return Add(this, step);
            
            return base.Add(other);
        }
        public static RrFunction Add(StepFunction step, ConstantRrFunction cst)
        {
            if (DoubleUtils.EqualZero(cst.Value))
                return step;

            var shiftedValues = step.values.Map(v => v + cst.Value);
            return new StepFunction(step.abscissae, shiftedValues, step.leftValue + cst.Value);
        }
        public static RrFunction Add(StepFunction left, StepFunction right)
        {
            return BinaryOp(left, right, (l, r) => l + r);
        }

        public override RrFunction Mult(RrFunction other)
        {
            var cst = other as ConstantRrFunction;
            if (cst != null)
                return Mult(this, cst);

            var step = other as StepFunction;
            if (step != null)
                return Mult(this, step);

            return new WeightedStepFunction(other, abscissae, values, leftValue);
        }
        public static RrFunction Mult(StepFunction step, ConstantRrFunction cst)
        {
            if (DoubleUtils.MachineEquality(cst.Value, 1.0))
                return step;

            if (DoubleUtils.EqualZero(cst.Value))
                return RrFunctions.Zero;

            var multValues = step.values.Map(v => v * cst.Value);
            return new StepFunction(step.abscissae, multValues, step.leftValue * cst.Value);
        }
        public static RrFunction Mult(StepFunction leftStep, StepFunction rightStep)
        {
            return BinaryOp(leftStep, rightStep, (l, r) => l * r);
        }
    }
    
    public class WeightedStepFunction : RrFunction  
    {
        #region private fields
        private readonly RrFunction weight;
        private readonly StepFunction<double> stepEval;
        
        private readonly double[] abscissae;
        private readonly double[] values;
        private readonly double leftValue;
        #endregion
        public WeightedStepFunction(RrFunction weight, double[] abscissae, double[] values, double leftValue)
        {
            this.weight = weight;
            this.abscissae = abscissae;
            this.values = values;
            this.leftValue = leftValue;
            stepEval = new StepFunction<double>(abscissae, values, leftValue);
        }
        
        public override double Eval(double x)
        {
            return stepEval.Eval(x) * weight.Eval(x);
        }

        public override RrFunction Integral(double basePoint)
        {
            var weightIntegral = weight.Integral(basePoint);
            
            var stepPartVals = new double[abscissae.Length];
            double prev = 0.0;
            for (int i = 0; i < abscissae.Length; i++)
            {
                prev += weightIntegral.Eval(abscissae[i]) * (values[i] - (i > 0 ? values[i - 1] : leftValue));
                stepPartVals[i] = -prev;
            }
            
            RrFunction stepPart = new StepFunction(abscissae, stepPartVals, 0.0);

            var unbasedResult = new WeightedStepFunction(weightIntegral, abscissae, values, leftValue) + stepPart ;
            return unbasedResult - unbasedResult.Eval(basePoint);
        }
        public override RrFunction Derivative()
        {
            throw new Exception("WeightedStepFunction derivative is not a function ");
        }
        public override RrFunction Inverse()
        {
            return new WeightedStepFunction(weight.Inverse(), abscissae, values.Map(v => 1.0 / v), 1.0 / leftValue);
        }
        public override RrFunction Mult(RrFunction other)
        {
            var resultWeight = weight.Mult(other);
            return new WeightedStepFunction(resultWeight, abscissae, values, leftValue);
        }
    }

}