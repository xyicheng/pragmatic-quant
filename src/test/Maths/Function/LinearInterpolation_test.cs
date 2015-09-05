using System;
using System.Linq;
using NUnit.Framework;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths;
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

            var stepFunc = RrFunctions.LinearInterpolation(abs, vals, leftSlope, rightSlope);

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

            var stepFunc = SplineInterpoler.BuildCubicSpline(abs, vals);

            foreach (var i in Enumerable.Range(0, abs.Length-1))
            {
                Assert.AreEqual(stepFunc.Eval(abs[i]), vals[i]);
            }
        }

        [Test]
        public void Add()
        {
            var cubic1 = SplineInterpoler.BuildCubicSpline(new[] { 0.0, 1.0, 2.0, 3.0 }, new[] { 1.0, 0.22, -0.1, 0.1 });
            var cubic2 = SplineInterpoler.BuildCubicSpline(new[] { -0.5, 1.1, 2.0, 5.0 }, new[] { 1.0, 0.22, -0.1, 0.1 });
            var add = cubic1 + cubic2;

            Assert.IsTrue(add is SplineInterpoler);

            var testVals = new[] {-0.5, 0.0, 1.0, 1.1, 2.0, 3.0, 5.0};
            foreach (var x in testVals)
            {
                var err = Math.Abs(add.Eval(x) - (cubic1.Eval(x) + cubic2.Eval(x)));
                Assert.AreEqual(err, 0.0);
            }

            var rand = new Random(251);
            for (int i = 0; i < 100; i++)
            {
                double x = -5.0 + 12.0 * rand.NextDouble();
                var err = Math.Abs(add.Eval(x) - (cubic1.Eval(x) + cubic2.Eval(x)));
                Assert.LessOrEqual(err, 9.0 * DoubleUtils.MachineEpsilon);
            }
        }

        [Test]
        public void Mult()
        {
            var cubic1 = SplineInterpoler.BuildCubicSpline(new[] { 0.0, 1.0, 2.0, 3.0 }, new[] { 1.0, 0.22, -0.1, 0.1 });
            var cubic2 = SplineInterpoler.BuildCubicSpline(new[] { -0.5, 1.1, 2.0, 5.0 }, new[] { 1.0, 0.22, -0.1, 0.1 });
            var mult = cubic1 * cubic2;

            Assert.IsTrue(mult is SplineInterpoler);

            var testVals = new[] { -0.5, 0.0, 1.0, 1.1, 2.0, 3.0, 5.0 };
            foreach (var x in testVals)
            {
                var err = Math.Abs(mult.Eval(x) - (cubic1.Eval(x) * cubic2.Eval(x)));
                Assert.AreEqual(err, 0.0);
            }
            
            var rand = new Random(251);
            for (int i = 0; i < 100; i++)
            {
                double x = -5.0 + 12.0 * rand.NextDouble();
                var err = Math.Abs(mult.Eval(x) - (cubic1.Eval(x) * cubic2.Eval(x)));
                Assert.LessOrEqual(err, 18.0 * DoubleUtils.MachineEpsilon);
            }
        }
    }
}
