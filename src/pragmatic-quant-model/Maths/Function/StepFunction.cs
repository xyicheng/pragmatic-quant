using System;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths.Interpolation;

namespace pragmatic_quant_model.Maths.Function
{
    public class StepFunction : RrFunction
    {
        #region private fields
        private readonly StepSearcher stepSearcher;
        private readonly double[] abscissae;
        private readonly double[] values;
        private readonly double leftValue;
        #endregion
        public StepFunction(double[] abscissae, double[] values, double leftValue)
        {
            this.abscissae = abscissae;
            this.values = values;
            this.leftValue = leftValue;
            stepSearcher = new StepSearcher(abscissae);
        }
        public override double Eval(double x)
        {
            int leftIndex = stepSearcher.LocateLeftIndex(x);

            if (leftIndex <= -1)
                return leftValue;

            return values[Math.Min(leftIndex, abscissae.Length - 1)];
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
            {
                if (DoubleUtils.MachineEquality(cst.Value,1.0))
                    return this;

                if (DoubleUtils.EqualZero(cst.Value))
                    return RrFunctions.Zero;

                var multValues = values.Map(v => v * cst.Value);
                return new StepFunction(abscissae, multValues, leftValue + cst.Value);
            }

            var step = other as StepFunction;
            if (step != null)
            {
                var mergedAbscissae = abscissae.Union(step.abscissae).OrderBy(p => p).ToArray();
                var multValues = mergedAbscissae.Map(p => Eval(p) * step.Eval(p));
                var multLeftValue = Eval(double.NegativeInfinity) * step.Eval(double.NegativeInfinity);
                return new StepFunction(mergedAbscissae, multValues, multLeftValue);
            }

            return base.Mult(other);
        }
    }

}