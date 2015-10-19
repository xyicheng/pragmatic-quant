using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using pragmatic_quant_model.Product.DslPayoff;
using pragmatic_quant_model.Product.Fixings;

namespace test.Product
{
    [TestFixture]
    public class DslPayoffTest
    {
        [Test, TestCaseSource("Payoffs")]
        public void MonoPayoffTest(string payoffScript, IDictionary<string, object> parameters, Func<double, double> refPayoff)
        {
            DslPayoffExpression payoffExpr = DslPayoffParser.Parse(payoffScript, parameters);
            IFixingFunction payoff = DslPayoffCompiler.Compile(payoffExpr).First();

            var rand = new Random(4321);
            for (int i = 0; i < 100; i++)
            {
                var fixingValue = 2.0 * rand.NextDouble();
                var cpnValue = payoff.Value(new[] { fixingValue });
                var refCpnValue = refPayoff(fixingValue);
                Assert.AreEqual(cpnValue, refCpnValue);
            }
        }

        public static IEnumerable Payoffs()
        {
            #region Payoff1
            const string payoffScript1 = "Max(0.0,  1.2 * stock@fixingdate - strike)";
            var parameters1 = new Dictionary<string, object>
            {
                {"stock", "asset(eurostoxx,EUR)"},
                {"fixingdate", new DateTime(2015, 07, 31)},
                {"strike", 0.95}
            };
            Func<double, double> refPayoff1 = x => Math.Max(0.0, 1.2 * x - 0.95);
            yield return new object[] {payoffScript1, parameters1, refPayoff1};
            #endregion

            #region Payoff2
            const string payoffScript2 = "(stock@fixingdate > strike) ? 1.2 * stock@fixingdate - strike + 0.01 : 0.0";
            var parameters2 = new Dictionary<string, object>
            {
                {"stock", "asset(eurostoxx,EUR)"},
                {"fixingdate", new DateTime(2015, 07, 31)},
                {"strike", 0.95}
            };
            Func<double, double> refPayoff2 = x => x > 0.95 ? 1.2 * x - 0.95 + 0.01 : 0.0;
            yield return new object[] { payoffScript2, parameters2, refPayoff2 };
            #endregion

            #region Payoff3
            const string payoffScript3 = "-1.0 * stock@fixingdate";
            var parameters3 = new Dictionary<string, object>
            {
                {"stock", "asset(eurostoxx,EUR)"},
                {"fixingdate", new DateTime(2015, 07, 31)},
                {"strike", 0.95}
            };
            Func<double, double> refPayoff3 = x => -x;
            yield return new object[] { payoffScript3, parameters3, refPayoff3 };
            #endregion
        }

    }

}
