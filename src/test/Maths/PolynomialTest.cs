using System;
using NUnit.Framework;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths;

namespace test.Maths
{
    [TestFixture]
    public class PolynomialTest
    {
        [Test]
        public void Constructor()
        {
            var p = new Polynomial(true, 0.0, 1.0, 0.0, 0.0, 0.0);
            Assert.AreEqual(p.Degree, 1);
            Assert.AreEqual(p.Coeffs[0], 0.0);
            Assert.AreEqual(p.Coeffs[1], 1.0);

            var zero = new Polynomial(0.0);
            Assert.AreEqual(zero.Degree, 0);
            Assert.AreEqual(zero.Coeffs[0], 0.0);

            var zero2 = new Polynomial(true, 0.0, 0.0);
            Assert.AreEqual(zero2.Degree, 0);
            Assert.AreEqual(zero2.Coeffs[0], 0.0);

            Polynomial cst = 5.0;
            Assert.AreEqual(cst.Degree, 0);
            Assert.AreEqual(cst.Coeffs[0], 5.0);
        }

        [Test]
        public void SimpleAddAndMult()
        {
            var x = Polynomial.X;
            var p = 1.0 + 2.0 * x + 3.0 * x * x + 4.0 * x * x * x;
            Assert.AreEqual(p.Coeffs.Length, 4);
            Assert.AreEqual(p.Coeffs[0], 1.0);
            Assert.AreEqual(p.Coeffs[1], 2.0);
            Assert.AreEqual(p.Coeffs[2], 3.0);
            Assert.AreEqual(p.Coeffs[3], 4.0);
        }

        [Test]
        public void Sub()
        {
            var p = new Polynomial(1.0, 1.0, 1.0, 1.0, 1.0);
            var zero = p - p;
            Assert.IsTrue(zero.IsZero());

            var zero2 = p + (-p);
            Assert.IsTrue(zero2.IsZero());
        }

        [Test]
        public void Mult()
        {
            var x = Polynomial.X;

            var p1 = x + 2.0;
            var p2 = x * x * x + 3.0 * x * x + 4.0 * x - 17.0;
            var product = p1 * p2;
            var result = x * x * x * x + 5.0 * x * x * x + 10.0 * x * x - 9 * x - 34;
            for (int i = 0; i < product.Coeffs.Length; i++)
                Assert.AreEqual(product.Coeffs[i], result.Coeffs[i]);
        }

        [Test]
        public void Deriv()
        {
            var x = Polynomial.X;
            var p = x * x * x + 3.0 * x * x + 4.0 * x - 17.0;
            var dp = p.Derivative();
            Assert.AreEqual(dp.Coeffs.Length, 3);
            Assert.AreEqual(dp.Coeffs[0], 4.0);
            Assert.AreEqual(dp.Coeffs[1], 6.0);
            Assert.AreEqual(dp.Coeffs[2], 3.0);
        }

        [Test]
        public void Taylor()
        {
            var x = Polynomial.X;

            var squareTaylor = (x * x).TaylorDev(1.0);
            var squareTaylorRef = 1.0 + 2.0 * x + x * x;
            Assert.IsTrue((squareTaylor - squareTaylorRef).IsZero());

            var fourTaylor = ((x - 0.5) * (x - 0.5) * (x - 0.5) * (x - 0.5)).TaylorDev(0.5);
            Assert.IsTrue((fourTaylor - x * x * x * x).IsZero());

            var p = 0.25 * x * x * x * x + x * x * x + 3.0 * x * x + 4.0 * x - 17.0;
            var taylorZero = p.TaylorDev(0.0);
            Assert.IsTrue((taylorZero - p).IsZero());

            var taylorOne = p.TaylorDev(1.0);
            var testVals = new[] {-1.0, -0.5, -0.01, 0.02, 1.0, 0.55};
            foreach (var val in testVals)
            {
                var err = Math.Abs(p.Eval(val) - taylorOne.Eval(val - 1.0));
                Assert.AreEqual(err, 0.0);
            }
        }

        [Test]
        public void Eval()
        {
            var x = Polynomial.X;
            var p = 1.5 + 2.0 * x - 0.01 * x * x + 0.5 * x * x * x;

            const double y = Math.PI * 0.01;
            var p_y = p.Eval(y);
            const double p_yref = 1.5 + 2.0 * y - 0.01 * y * y + 0.5 * y * y * y;
            var err = Math.Abs(p_y / p_yref - 1.0);
            Assert.LessOrEqual(err, DoubleUtils.MachineEpsilon);
        }

        [Test]
        public void TestToString()
        {
            var cst = new Polynomial(5);
            var cstAsString = cst.ToString();
            Assert.AreEqual(cstAsString, 5.ToString());

            var x = Polynomial.X;
            var xAsString = x.ToString();
            Assert.AreEqual(xAsString, "X");

            var squareAsString = (x * x).ToString();
            Assert.AreEqual(squareAsString, "X^2");

            var p = x + 5.0 * x * x;
            var pAsString = p.ToString();
            Assert.AreEqual(pAsString, string.Format("X + {0} * X^2", 5.0));
        }
    }
}
