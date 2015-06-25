using System;
using System.Collections;
using NUnit.Framework;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Model.BlackScholes;

namespace test.Model
{

    [TestFixture]
    public class BsDivModelTest
    {
        [Test, TestCaseSource(typeof(BsDivModelTest), "CheckForward")]
        public void CheckForward(AssetMarket assetMkt)
        {
            var refDate = DateTime.Parse("07/06/2009");

            var asset = new AssetId("Stoxx50", Currency.Eur);
            var sigma = RrFunctions.Constant(0.0);
            var divDates = EnumerableUtils.For(1, 12, i => refDate + i * Duration.Month);
            var divs = divDates.Map(d => DiscreteLocalDividend.AffineDividend(d, 0.03, 0.02));

            var bsModel = new BlackScholesModel(TimeMeasure.Act365(refDate), asset, sigma, divs);
            
        }
    }

    public class BsDivModelTestDatas
    {
        public IEnumerable CheckForward
        {
            get { yield return new object[] {null}; }
        }
    }

}
