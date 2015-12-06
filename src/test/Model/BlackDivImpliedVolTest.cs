using System;
using NUnit.Framework;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Model.Equity.Dividends;

namespace test.Model
{
    [TestFixture]
    public class BlackDivImpliedVolTest
    {
        [Test, TestCaseSource(typeof (BsDivModelTestDatas), "AssetMarket")]
        public void SampleStrike(AssetMarket assetMkt)
        {
            DateTime maturity = assetMkt.RefDate + 3 * Duration.Year;
            var fwd = assetMkt.Forward().Fwd(maturity);
            double t = assetMkt.Time[maturity];
            var pricer = BlackScholesWithDividendOption.Build(assetMkt.Spot, assetMkt.Dividends, assetMkt.RiskFreeDiscount, assetMkt.Time);
            
            var vols = GridUtils.RegularGrid(0.05, 1.0, 50);
            var moneynesses = GridUtils.RegularGrid(-10.0, 10.0, 51);
            foreach (double vol in vols)
            {
                var stdDev = vol * Math.Sqrt(t);
                foreach (double m in moneynesses)
                {
                    var strike = fwd * Math.Exp(stdDev * m);
                    var price = pricer.Price(t, strike, vol, m > 0 ? 1 : -1);
                    var impliedVol = pricer.ImpliedVol(t, strike, price, m > 0 ? 1 : -1);

                    var errRelative = (impliedVol - vol) / vol;
                    Assert.IsTrue(Math.Abs(errRelative) < 5.0e-13);
                }
            }
            
        }

        [Test, TestCaseSource(typeof(BsDivModelTestDatas), "AssetMarket")]
        public void CalibrationTest(AssetMarket assetMkt)
        {
            var maturities = new [] {3 * Duration.Month, 6 * Duration.Month, Duration.Year, 2 * Duration.Year, 3 * Duration.Year, 4 * Duration.Year, 5 * Duration.Year};
            var vols = new[] {0.20, 0.15, 0.20, 0.15, 0.20, 0.18, 0.20};
            
            var calibDates = assetMkt.Time[maturities.Map(p => assetMkt.RefDate + p)];
            var strikes = ArrayUtils.Constant(calibDates.Length, assetMkt.Spot);
            
            var pricer = BlackScholesWithDividendOption.Build(assetMkt.Spot, assetMkt.Dividends, assetMkt.RiskFreeDiscount, assetMkt.Time);
            var targetPrices = EnumerableUtils.For(0, calibDates.Length, 
                i => pricer.Price(calibDates[i], strikes[i], vols[i], 1.0));

            var calibVols = pricer.CalibrateVol(calibDates, targetPrices, assetMkt.Spot, 1.0);



        }


    }
}
