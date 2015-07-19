using System;
using System.Collections;
using NUnit.Framework;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths.Sobol;
using pragmatic_quant_model.Model;
using pragmatic_quant_model.Model.BlackScholes;
using pragmatic_quant_model.Pricing;

namespace test.Model
{

    [TestFixture]
    public class BsDivModelTest
    {
        [Test, TestCaseSource(typeof(BsDivModelTestDatas), "AssetMarket")]
        public void CheckForward(AssetMarket assetMkt)
        {
            var zcCurve = DiscountCurve.Flat(FinancingId.RiskFree(Currency.Eur), assetMkt.RefDate, 0.01);
            var market = new Market(new[] {zcCurve}, new[] {assetMkt});

            var zeroVol = new MapRawDatas<DateOrDuration, double>(new[] {new DateOrDuration(assetMkt.RefDate)}, new[] {0.0});
            var blackScholesDesc = new BlackScholesModelDescription(assetMkt.Asset, zeroVol, true);
            var mcConfig = new MonteCarloConfig(1, SobolDirection.Kuo3);

            var blackScholesModel = ModelFactories.For(blackScholesDesc).Build(blackScholesDesc, market);
            var mcPricer = McPricer.For(blackScholesDesc, mcConfig);
            //var priceResult = mcPricer.Price(product, blackScholesModel, market);

            var assetFwdCurve = assetMkt.Forward(zcCurve);

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
                var repo = DiscountCurve.Flat(FinancingId.RiskFree(asset.Currency), refDate, 0.03);
                
                var divDates = EnumerableUtils.For(1, 12, i => refDate + i * Duration.Month);
                var divQuotes = divDates.Map(d => new DividendQuote(d, 0.03, 0.02));

                var assetMarket = new AssetMarket(asset, refDate, time, 1.0, repo, divQuotes, null);
                yield return new object[] { assetMarket };
            }
        }
    }

}
