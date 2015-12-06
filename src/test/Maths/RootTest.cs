using System;
using System.Collections;
using NUnit.Framework;
using pragmatic_quant_model.Maths;

namespace test.Maths
{
    [TestFixture]
    public class RootTest
    {
        [TestCaseSource("BracketDatas")]
        public void BracketTest(Func<double, double> f, double x1, double x2)
        {
            double xa, xb;
            RootUtils.Bracket(f, x1, x2, out xa, out xb);
            Assert.IsTrue(f(xa) * f(xb) < 0.0);
        }

        public static IEnumerable BracketDatas()
        {
            Func<double, double> f = x => x * x - 9;
            yield return new object[] { f, 0.0, 1.0 };
            yield return new object[] { f, 1.0, 0.0 };
            yield return new object[] { f, 5.0, 4.0 };
            yield return new object[] { f, 4.0, 5.0 };
            yield return new object[] { f, 2.0, 4.0 };
            yield return new object[] { f, 3.0, 4.0 };

            Func<double, double> minusf = x => -f(x);
            yield return new object[] { minusf, 0.0, 1.0 };
            yield return new object[] { minusf, 1.0, 0.0 };
            yield return new object[] { minusf, 5.0, 4.0 };
            yield return new object[] { minusf, 4.0, 5.0 };
            yield return new object[] { minusf, 2.0, 4.0 };
            yield return new object[] { minusf, 3.0, 4.0 };
        }

    }
;}
