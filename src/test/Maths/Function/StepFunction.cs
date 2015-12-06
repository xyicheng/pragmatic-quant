using System;
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

        [TestCase(new[] {0.0, 1.0, 2.0, 3.0}, new[] {5.1, 5.4, 3.1, 1.0}, 0.1, new[] {0.0, 5.1, 10.5, 13.6})]
        [TestCase(new[] {-1.0, 0.5, 5.0, 10.0}, new[] {5.0, 5.4, 3.1, -1.0}, -0.1, new[] {0.0, 7.5, 31.8, 47.3})]
        public void TestIntegral(double[] pillars, double[] vals, double leftVal, double[] refIntegralVals)
        {
            var stepFunc = new StepFunction(pillars, vals, leftVal);
            var integral = stepFunc.Integral(pillars[0]);

            Assert.IsTrue(integral is SplineInterpoler);
            for (int i = 0; i < pillars.Length - 1; i++)
            {
                var integralVal = integral.Eval(pillars[i]);
                Assert.IsTrue(DoubleUtils.MachineEquality(integralVal, refIntegralVals[i]));
            }
        }

        [Test]
        public void TestMult()
        {
            var pillars1 = new[] {0.0, 1.0, 2.0};
            var vals1 = new[] {0.007, 0.004, 0.0065};
            const double left1 = 2.0;
            var step1 = new StepFunction(pillars1, vals1, left1);

            var pillars2 = new[] {-0.8, 0.2, 1.0, 1.5, 2.0, 3.55};
            var vals2 = new[] {0.005, 0.003, -0.1, 0.0, -0.08, 10.0};
            const double left2 = -1.89;
            var step2 = new StepFunction(pillars2, vals2, left2);
            
            var prod = step1 * step2;
            Assert.IsTrue(prod is StepFunction);

            var allPillars = pillars1.Union(pillars2).OrderBy(p => p).ToArray();
            for (int i = 0; i < allPillars.Length; i++)
            {
                double x = allPillars[i];
                var val1 = step1.Eval(x);
                var val2 = step2.Eval(x);
                var prodVal = prod.Eval(x);
                
                Assert.IsTrue(DoubleUtils.MachineEquality(prodVal, val1 * val2));
            }
            Assert.IsTrue(DoubleUtils.MachineEquality(prod.Eval(double.NegativeInfinity),
                step1.Eval(double.NegativeInfinity) * step2.Eval(double.NegativeInfinity)));

            var rand = new Random(123);
            double a = allPillars.Max() - allPillars.Min() ;
            double b = allPillars.Min();
            for (int i = 0; i < 1000; i++)
            {
                var x = rand.NextDouble() * a * 3.0 + b - 0.1 * a;
                var val1 = step1.Eval(x);
                var val2 = step2.Eval(x);
                var prodVal = prod.Eval(x);

                Assert.IsTrue(DoubleUtils.MachineEquality(prodVal, val1 * val2));
            }

        }
        
        [Test]
        public void TestAdd()
        {
            var pillars1 = new[] { 0.0, 1.0, 2.0 };
            var vals1 = new[] { 0.007, 0.004, 0.0065 };
            const double left1 = 2.0;
            var step1 = new StepFunction(pillars1, vals1, left1);

            var pillars2 = new[] { -0.8, 0.2, 1.0, 1.5, 2.0, 3.55 };
            var vals2 = new[] { 0.005, 0.003, -0.1, 0.0, -0.08, 10.0 };
            const double left2 = -1.89;
            var step2 = new StepFunction(pillars2, vals2, left2);

            var sum = step1 + step2;
            Assert.IsTrue(sum is StepFunction);

            var allPillars = pillars1.Union(pillars2).OrderBy(p => p).ToArray();
            for (int i = 0; i < allPillars.Length; i++)
            {
                double x = allPillars[i];
                var val1 = step1.Eval(x);
                var val2 = step2.Eval(x);
                var sumVal = sum.Eval(x);

                Assert.IsTrue(DoubleUtils.MachineEquality(sumVal, val1 + val2));
            }
            Assert.IsTrue(DoubleUtils.MachineEquality(sum.Eval(double.NegativeInfinity),
                step1.Eval(double.NegativeInfinity) + step2.Eval(double.NegativeInfinity)));

            var rand = new Random(123);
            double a = allPillars.Max() - allPillars.Min();
            double b = allPillars.Min();
            for (int i = 0; i < 1000; i++)
            {
                var x = rand.NextDouble() * a * 3.0 + b - 0.1 * a;
                var val1 = step1.Eval(x);
                var val2 = step2.Eval(x);
                var sumVal = sum.Eval(x);

                Assert.IsTrue(DoubleUtils.MachineEquality(sumVal, val1 + val2));
            }

        }
        
    }

    [TestFixture]
    public class WeightedStepFunctionTest
    {

        [Test]
        public void TestIntegralExp()
        {
            var exp = RrFunctions.Exp(0.1);
            var step = new StepFunction(new[] {0.0, 5.0}, new[] {0.015, 0.010}, 0.0);
            var f = exp * step;
            
            var integral = f.Integral(0.0);
            var testVal = integral.Eval(10.0);

            var expintegral = exp.Integral(0.0);
            var refVal = 0.015 * (expintegral.Eval(5.0) - expintegral.Eval(0.0))
                       + 0.01 * (expintegral.Eval(10.0) - expintegral.Eval(5.0));

            Assert.IsTrue(DoubleUtils.Equality(testVal, refVal, 1.5 * DoubleUtils.MachineEpsilon));
        }

    }


}
