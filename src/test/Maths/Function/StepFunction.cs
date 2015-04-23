using System.Linq;
using NUnit.Framework;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths.Function;

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
    }
}
