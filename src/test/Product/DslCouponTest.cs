using System;
using System.Collections.Generic;
using NUnit.Framework;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Product;
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
            var coupon = DslCoupon.Parse(payment, couponParameters, payoffScript);

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
