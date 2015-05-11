using System.Linq;
using NUnit.Framework;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths.Function;
using pragmatic_quant_model.Maths.Interpolation;

namespace test.Maths.Function
{
    [TestFixture]
    public class StepFunction_Test
    {
        [Test]
        public void TestEval()
        {
            var abs = new[] {0.0, 0.5, 0.99, 2.5};
            var vals = new[] {5.0, 5.4, 3.1, 1.0};
            const double leftVal = 0.0;

            var stepFunc = new StepFunction(abs, vals, leftVal);

            foreach (var i in Enumerable.Range(0, abs.Length))
            {
                Assert.AreEqual(stepFunc.Eval(abs[i]), vals[i]);
                if (i < abs.Length - 1)
                {
                    var midPoint = 0.5 * (abs[i] + abs[i + 1]);
                    Assert.AreEqual(stepFunc.Eval(midPoint), vals[i]);

                    var nextPointMinus = abs[i] + (1.0 - 5.0 * DoubleUtils.MachineEpsilon) * (abs[i + 1] - abs[i]);
                    Assert.AreEqual(stepFunc.Eval(nextPointMinus), vals[i]);
                }
            }

            Assert.AreEqual(stepFunc.Eval(abs[0] - 0.00000000001), leftVal);
            Assert.AreEqual(stepFunc.Eval(double.NegativeInfinity), leftVal);
            Assert.AreEqual(stepFunc.Eval(abs[abs.Length - 1] + 0.00000000001), vals[abs.Length - 1]);
            Assert.AreEqual(stepFunc.Eval(double.PositiveInfinity), vals[abs.Length - 1]);
        }

        [TestCase(new[] { 0.0, 0.5, 0.99, 2.5 }, new[] { 5.0, 5.4, 3.1, 1.0 }, 0.1)]
        [TestCase(new[] { -1.0, 0.5, 5.0, 10.0 }, new[] { 5.0, 5.4, 3.1, -1.0 }, -0.1)]
        public void TestIntegral(double[] pillars, double[] vals, double leftVal)
        {
            var stepFunc = new StepFunction(pillars, vals, leftVal);
            var integral = stepFunc.Integral(0.0);

            Assert.IsTrue(integral is LinearInterpolation); 
            for (int i = 0; i < pillars.Length-1; i++)
            {
                var mid = 0.5 * (pillars[i] + pillars[i + 1]);
                var midIntegral = integral.Eval(mid);
                var midIntegralRef = 0.5 * (integral.Eval(pillars[i]) + integral.Eval(pillars[i + 1]));
                Assert.IsTrue(DoubleUtils.Equality(midIntegral, midIntegralRef, 1.1 * DoubleUtils.MachineEpsilon));
            }

            var leftPoint = pillars[0] - 1.0;
            var lefIntegral = integral.Eval(leftPoint);
            var lefIntegralRef = integral.Eval(pillars[0]) - leftVal;
            Assert.IsTrue(DoubleUtils.Equality(lefIntegral, lefIntegralRef, 1.1 * DoubleUtils.MachineEpsilon));

            var rightPoint = pillars[pillars.Length-1] + 1.0;
            var rightIntegral = integral.Eval(rightPoint);
            var rightIntegralRef = integral.Eval(pillars[pillars.Length - 1]) + vals[pillars.Length - 1];
            Assert.IsTrue(DoubleUtils.Equality(rightIntegral, rightIntegralRef, 1.1 * DoubleUtils.MachineEpsilon));
        }
    }
}
