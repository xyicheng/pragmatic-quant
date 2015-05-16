using System.Linq;
using NUnit.Framework;
using pragmatic_quant_model.Maths.Interpolation;

namespace test.Maths.Function
{
    [TestFixture]
    public class LinearInterpolationTest
    {
        [Test]
        public void TestEval()
        {
            var abs = new[] { 0.0, 0.5, 0.99, 2.5 };
            var vals = new[] { 5.0, 5.4, 3.1, 1.0 };
            const double leftSlope = 0.0;
            const double rightSlope = 0.98181;

            var stepFunc = new LinearInterpolation(abs, vals, leftSlope, rightSlope);

            foreach (var i in Enumerable.Range(0, abs.Length))
            {
                Assert.AreEqual(stepFunc.Eval(abs[i]), vals[i]);
                if (i < abs.Length - 1)
                {
                    var midPoint = 0.5 * (abs[i] + abs[i + 1]);
                    Assert.AreEqual(stepFunc.Eval(midPoint), 0.5 * (vals[i] + vals[i + 1]));
                }
            }

            double leftExtrapolPoint = abs[0] - 10.0;
            Assert.AreEqual(stepFunc.Eval(leftExtrapolPoint), vals[0] + leftSlope * (leftExtrapolPoint - abs[0]));

            double rightExtrapolPoint = abs[abs.Length-1] + 10.0;
            Assert.AreEqual(stepFunc.Eval(rightExtrapolPoint), vals[abs.Length - 1] + rightSlope * (rightExtrapolPoint - abs[abs.Length - 1]));
        }
    }

    [TestFixture]
    public class CubicSplineInterpolationTest
    {
        [Test]
        public void TestEval()
        {
            var abs = new[] {0.0, 0.5, 0.99, 2.5};
            var vals = new[] {5.0, 5.4, 3.1, 1.0};

            var stepFunc = new CubicSplineInterpoler(abs, vals, double.NaN, double.NaN);

            foreach (var i in Enumerable.Range(0, abs.Length-1))
            {
                Assert.AreEqual(stepFunc.Eval(abs[i]), vals[i]);
            }
        }
    }
}
