using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Product.CouponDsl;

namespace test.Product
{
    [TestFixture]
    public class DslCouponTest
    {
        [Test]
        public void MonoCallPayoff()
        {
            const string payoffScript = "Max(0.0,  1.2 * stock@fixingdate - strike)";
            var couponParameters = new Dictionary<string, object>
            {
                {"stock", "asset(eurostoxx,EUR)"},
                {"fixingdate", new DateTime(2015, 07, 31)},
                {"strike", 0.95}
            };
            var payment = new PaymentInfo(Currency.Eur, new DateTime(2015, 07, 31));
            
            DslPayoffExpression payoff = DslPayoffParser.Parse(payoffScript, couponParameters);
            var dslCouponData = new DslCouponData(payoff, payment);
            var coupon =  DslCouponCompiler.Compile(dslCouponData).First();

            var rand = new Random(4321);
            for (int i = 0; i < 100; i++)
            {
                var fixingValue = 2.0 * rand.NextDouble();
                var cpnValue = coupon.Payoff(new[] {fixingValue});
                var refCpnValue = Math.Max(0.0, 1.2 * fixingValue - 0.95);

                Assert.AreEqual(cpnValue, refCpnValue);
            }
        }
    }
}
