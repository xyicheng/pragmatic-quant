using System;
using NUnit.Framework;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Model.Equity;

namespace test.Model
{
    [TestFixture]
    public class BlackDivImpliedVol
    {
        [Test, TestCaseSource(typeof (BsDivModelTestDatas), "AssetMarket")]
        public void SampleStrike(AssetMarket assetMkt)
        {
            DateTime maturity = assetMkt.RefDate + 2 * Duration.Year;
            var fwd = assetMkt.Forward().Fwd(maturity);
            double t = assetMkt.Time[maturity];
            var pricer = BlackScholesWithDividendOption.Build(assetMkt.Spot, assetMkt.Dividends, assetMkt.RiskFreeDiscount, assetMkt.Time);
            
            const double vol = 0.25387644;

            var moneynesses = GridUtils.RegularGrid(-3.0, 3.0, 10);
            foreach (double m in moneynesses)
            {
                var strike = fwd * Math.Exp(m);
                var price = pricer.Price(t, strike, vol, m > 0 ? 1 : -1);
                var impliedVol = pricer.ImpliedVol(t, strike, price, m > 0 ? 1 : -1);

                var errRelative = (impliedVol - vol) / vol;
                Assert.IsTrue(Math.Abs(errRelative) < 1.0e-9);
            }
        }
    }
}
