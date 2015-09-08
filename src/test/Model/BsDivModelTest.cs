using System;
using System.Linq;
using System.Collections;
using pragmatic_quant_com.Factories;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Sobol;
using pragmatic_quant_model.Model;
using pragmatic_quant_model.Model.Equity.BlackScholes;
using pragmatic_quant_model.Pricing;
using pragmatic_quant_model.Product;
using NUnit.Framework;

namespace test.Model
{

    [TestFixture]
    public class BsDivModelTest
    {
        #region private fields
        private static IProduct ForwardLeg(DateTime[] fwdDates)
        {
            var legBag = new object[3 + fwdDates.Length, 4];
            var header = new object[,]
            {
                {"ProductName", "FwdLeg", null, null},
                {"FwdCouponScript", "stock@fixingDate", null, null},
                {"FwdPayDate", "PayCurrency", "Stock", "FixingDate"}
            };
            ArrayUtils.SetSubArray(ref legBag, 0, 0, header);
            
            for (int i = 0; i < fwdDates.Length; i++)
                ArrayUtils.SetRow(ref legBag, 3 + i, new object[] {fwdDates[i], "EUR", "asset(Stoxx50,EUR)", fwdDates[i]});
            
            var leg = ProductFactory.Instance.Build(legBag);
            return leg;
        }
        #endregion
        
        [Test, TestCaseSource(typeof(BsDivModelTestDatas), "AssetMarket")]
        public void CheckZeroVolForward(AssetMarket assetMkt)
        {
            var zcCurve = assetMkt.RiskFreeDiscount;
            var market = new Market(new[] {zcCurve}, new[] {assetMkt});

            var zeroVol = new MapRawDatas<DateOrDuration, double>(new[] {new DateOrDuration(assetMkt.RefDate)}, new[] {0.0});
            var blackScholesDesc = new BlackScholesModelDescription(assetMkt.Asset.Name, zeroVol, true);
            var mcConfig = new MonteCarloConfig(1, RandomGenerators.GaussianSobol(SobolDirection.Kuo3));

            var blackScholesModel = ModelFactories.For(blackScholesDesc).Build(blackScholesDesc, market);
            var mcPricer = McPricer.For(blackScholesDesc, mcConfig);

            var fwdDates = new[] {Duration.Month, 6 * Duration.Month, Duration.Year, 2 * Duration.Year, 5 * Duration.Year}
                                .Map(d => assetMkt.RefDate + d);

            IProduct fwdLeg = ForwardLeg(fwdDates);
            PriceResult priceResult = mcPricer.Price(fwdLeg, blackScholesModel, market);
            double[] fwds = priceResult.Details.Map(kv => kv.Value.Value);

            var assetFwdCurve = assetMkt.Forward();
            double[] refFwds = fwdDates.Map(d => assetFwdCurve.Fwd(d) * zcCurve.Zc(d));

            foreach (var i  in Enumerable.Range(0, fwdDates.Length))
            {
                var err = Math.Abs(fwds[i] / refFwds[i] - 1.0);
                Assert.LessOrEqual(err, 20.0 * DoubleUtils.MachineEpsilon);
            }
        }

    }

    public class BsDivModelTestDatas
    {
        public IEnumerable AssetMarket
        {
            get
            {
                var refDate = DateTime.Parse("07/06/2009");
                var asset = new AssetId("Stoxx50", Currency.Eur);
                var time = TimeMeasure.Act365(refDate);

                var zcCurve = DiscountCurve.Flat(FinancingId.RiskFree(Currency.Eur), refDate, 0.01);
                var repo = DiscountCurve.Flat(FinancingId.RiskFree(asset.Currency), refDate, 0.03);
                
                var divDates = EnumerableUtils.For(1, 60, i => refDate + i * Duration.Month);
                var divQuotes = divDates.Map(d => new DividendQuote(d, 0.03 / 12.0, 0.02));

                var assetMarket = new AssetMarket(asset, refDate, time, 1.0, zcCurve, repo, divQuotes, null);
                yield return new object[] { assetMarket };
            }
        }
    }

}
