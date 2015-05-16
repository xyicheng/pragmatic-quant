using System;
using System.Diagnostics;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths.Function;

namespace pragmatic_quant_model.Maths.Interpolation
{
    public class LinearInterpolation : RrFunction
    {
        #region private fields
        private readonly StepFunction<LinearElement> stepLinear;
        private readonly double[] abscissae;
        private readonly double[] values;
        private readonly double leftExtrapolationSlope;
        private readonly double rightExtrapolationSlope;
        #endregion
        public LinearInterpolation(double[] abscissae, double[] values, double leftExtrapolationSlope, double rightExtrapolationSlope)
        {
            this.abscissae = abscissae;
            this.values = values;
            this.leftExtrapolationSlope = leftExtrapolationSlope;
            this.rightExtrapolationSlope = rightExtrapolationSlope;

            var linearInterpol = Enumerable.Range(0, abscissae.Length - 1)
                .Select(i => new LinearElement(abscissae[i + 1] - abscissae[i], values[i], values[i + 1]));
            var rightElem = new LinearElement(values[values.Length - 1], rightExtrapolationSlope);
            var leftElem = new LinearElement(values[0], leftExtrapolationSlope);
            
            stepLinear = new StepFunction<LinearElement>(abscissae, linearInterpol.Concat(new[] {rightElem}).ToArray(), leftElem);
        }
        public override double Eval(double x)
        {
            int stepIndex;
            var linear = stepLinear.Eval(x, out stepIndex);
            var h = x - abscissae[Math.Max(0, stepIndex)];
            return linear.Value + linear.Slope * h;
        }
        public override RrFunction Add(RrFunction other)
        {
            var cst = other as ConstantRrFunction;
            if (cst != null)
            {
                var shiftedValues = values.Map(v => v + cst.Value);
                return new LinearInterpolation(abscissae, shiftedValues, leftExtrapolationSlope, rightExtrapolationSlope);
            }

            var linear = other as LinearInterpolation;
            if (linear != null)
            {
                var mergedAbscissae = abscissae.Union(linear.abscissae).OrderBy(p => p).ToArray();
                var addValues = mergedAbscissae.Map(p => Eval(p) + linear.Eval(p));
                return new LinearInterpolation(mergedAbscissae, addValues,
                    leftExtrapolationSlope + linear.leftExtrapolationSlope,
                    rightExtrapolationSlope + linear.rightExtrapolationSlope);
            }

            return base.Add(other);
        }
        public override RrFunction Mult(RrFunction other)
        {
            var cst = other as ConstantRrFunction;
            if (cst != null)
            {
                var multValues = values.Map(v => v * cst.Value);
                return new LinearInterpolation(abscissae,
                    multValues, leftExtrapolationSlope * cst.Value, rightExtrapolationSlope * cst.Value);
            }

            return base.Mult(other);
        }
    }

    /// <summary>
    /// Linear data :
    /// x -> Value + Slope*x 
    /// </summary>
    [DebuggerDisplay("{Value} + {Slope}*x")]
    public struct LinearElement
    {
        public LinearElement(double value, double slope)
        {
            Value = value;
            Slope = slope;
        }
        public LinearElement(double step, double leftVal, double rightVal)
        {
            Value = leftVal;
            Slope = (rightVal - leftVal) / step;
        }
        
        public readonly double Value;
        public readonly double Slope;
    }
}
