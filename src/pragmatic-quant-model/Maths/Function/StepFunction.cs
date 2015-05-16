﻿using System.Diagnostics.Contracts;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths.Interpolation;

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
            var zeroBasedIntegral = new LinearInterpolation(abscissae, zeroBaseIntegrals, leftValue, values[values.Length - 1]);
            return zeroBasedIntegral - zeroBasedIntegral.Eval(basePoint);
        }
        public override RrFunction Add(RrFunction other)
        {
            var cst = other as ConstantRrFunction;
            if (cst != null)
            {
                var shiftedValues = values.Map(v => v + cst.Value);
                return new StepFunction(abscissae, shiftedValues, leftValue + cst.Value);
            }

            var step = other as StepFunction;
            if (step != null)
            {
                var mergedAbscissae = abscissae.Union(step.abscissae).OrderBy(p => p).ToArray();
                var addValues = mergedAbscissae.Map(p => Eval(p) + step.Eval(p));
                var addLeftValue = Eval(double.NegativeInfinity) + step.Eval(double.NegativeInfinity);
                return new StepFunction(mergedAbscissae, addValues, addLeftValue);
            }

            return base.Add(other);
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
            var mergedAbscissae = leftStep.abscissae.Union(rightStep.abscissae).OrderBy(p => p).ToArray();
            var multValues = mergedAbscissae.Map(p => leftStep.Eval(p) * rightStep.Eval(p));
            var multLeftValue = leftStep.Eval(double.NegativeInfinity) * rightStep.Eval(double.NegativeInfinity);
            return new StepFunction(mergedAbscissae, multValues, multLeftValue);
        }
    }

    public class StepFunction<T>
    {
        #region private fields
        private readonly StepSearcher stepSearcher;
        private readonly T[] values;
        private readonly T leftValue;
        #endregion
        public StepFunction(double[] pillars, T[] values, T leftValue)
        {
            Contract.Requires(pillars.Length == values.Length);
            this.values = values;
            this.leftValue = leftValue;
            stepSearcher = new StepSearcher(pillars);
        }
        
        public T Eval(double x)
        {
            int stepIndex;
            return Eval(x, out stepIndex);
        }
        public T Eval(double x, out int stepIndex)
        {
            stepIndex = stepSearcher.LocateLeftIndex(x);

            if (stepIndex <= -1)
                return leftValue;

            return values[stepIndex];
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
            var weightIntegralPart = new WeightedStepFunction(weight.Integral(0.0),
                abscissae, values, leftValue);
            
            var stepPartVals = new double[abscissae.Length];
            double prev = 0.0;
            for (int i = 0; i < abscissae.Length; i++)
            {
                prev += weight.Eval(abscissae[i]) * (values[i] - (i > 0 ? values[i - 1] : leftValue));
                stepPartVals[i] = prev;
            }
            
            RrFunction stepPart = new StepFunction(abscissae, stepPartVals, 0.0);
            stepPart -= weightIntegralPart.Eval(basePoint) + stepPart.Eval(basePoint);

            return weightIntegralPart + stepPart;
        }
        public override RrFunction Mult(RrFunction other)
        {
            var resultWeight = weight.Mult(other);
            return new WeightedStepFunction(resultWeight, abscissae, values, leftValue);
        }
    }

}