using System;
using System.Collections;
using NUnit.Framework;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Function;
using pragmatic_quant_model.Maths.Sobol;
using pragmatic_quant_model.Model.HullWhite;

namespace test.Model
{
    [TestFixture]
    public class Hw1ModelTest
    {
        [Test, TestCaseSource(typeof(Hw1ModelTestDatas), "Zc")]
        public void Zc(double lambda, Duration zcStart, Duration zcDuration, int nbPaths, double precision)
        {
            var sigma = new StepFunction(new[] { 0.0, 1.0, 2.0 }, new[] { 0.007, 0.004, 0.0065 }, 0.0);
            var refDate = DateTime.Parse("07/06/2009");

            var hw1 = new Hw1Model(TimeMeasure.Act365(refDate), Currency.Eur, lambda, sigma);
            var hw1Zc = new Hw1ModelZcRepresentation(hw1);

            var zcDate = refDate + zcStart;
            var zc = hw1Zc.Zc(zcDate, zcDate + zcDuration, 1.0);

            var drift = hw1.DriftTerm();
            var stdDev = Math.Sqrt(drift.Eval(hw1.Time[zcDate]));
            
            //TODO replace by hermite quadrature
            var gaussGenerator = RandomGenerators.GaussianSobol(1, SobolDirection.JoeKuoD7);
            double zcQmc = 0.0;
            for (int i = 0; i < nbPaths; i++)
            {
                var ornstein = gaussGenerator.Next().Mult(stdDev);
                zcQmc  += zc.Eval(ornstein);
            }
            zcQmc /= nbPaths;
            

            var error = Math.Abs(zcQmc - 1.0);
            Assert.LessOrEqual(error, precision);
        }
    }

    internal class Hw1ModelTestDatas
    {
        public static IEnumerable Zc
        {
            get
            {
                yield return new object[] {0.01, 10 * Duration.Year, 30 * Duration.Year, 10000, 1.4e-4};
                yield return new object[] { 0.005, 30 * Duration.Year, 10 * Duration.Year, 10000, 1.2e-5 };
            }
        }
    }
}
