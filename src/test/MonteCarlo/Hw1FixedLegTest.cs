﻿using System;
using System.Linq;
using NUnit.Framework;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Function;
using pragmatic_quant_model.Maths.Sobol;
using pragmatic_quant_model.Model;
using pragmatic_quant_model.Model.HullWhite;
using pragmatic_quant_model.Pricing;
using pragmatic_quant_model.Product;
using pragmatic_quant_model.Product.Fixings;

namespace test.MonteCarlo
{
    [TestFixture]
    public class Hw1FixedLegTest
    {
        #region private methods
        private static Market Market()
        {
            var refDate = DateTime.Parse("07/06/2009");
            var time = TimeMeasure.Act365(refDate);
            var pillars = new[] {refDate + Duration.Year, refDate + 2 * Duration.Year, refDate + 3 * Duration.Year, refDate + 5 * Duration.Year};
            var zcRates = new[] { 0.0010, 0.003, 0.005, 0.008};
            var zcs = zcRates.Select((r, i) => Math.Exp(-time[pillars[i]] * r)).ToArray();

            DiscountCurve discountCurve = DiscountCurve.LinearRateInterpol(
                FinancingId.RiskFree(Currency.Eur),
                pillars, zcs, time);
            var market = new Market(new[] {discountCurve}, new AssetMarket[0]);
            return market;
        }
        private static Leg<Coupon> FixedLeg(DateTime refDate)
        {
            const int mat = 10;
            var dates = Enumerable.Range(0, mat).Select(i => refDate + (i + 1) * Duration.Year);
            var coupons = dates.Map(d => new Coupon(new PaymentInfo(Currency.Eur, d),
                new GenericFixingFunction(new IFixing[0], f => 1.0)));
            return new Leg<Coupon>(coupons);
        }
        #endregion

        [Test]
        public void Test()
        {
            var market = Market();

            const double lambda = 0.01;
            var sigma = new StepFunction(new[] {0.0, 1.0, 2.0}, new[] {0.007, 0.004, 0.0065}, 0.0);
            var hw1 = new Hw1Model(TimeMeasure.Act365(market.RefDate), Currency.Eur, lambda, sigma);
            var mcConfig = new MonteCarloConfig(20000,
                RandomGenerators.GaussianSobol(SobolDirection.JoeKuoD5));
            
            var mcPricer = McPricer.WithDetails(mcConfig);
            
            var fixedLeg = FixedLeg(market.RefDate);
            var mcPriceResult = (PriceResult) mcPricer.Price(fixedLeg, hw1, market);

            var mcCoupons = mcPriceResult.Details.Map(p => p.Item3.Value);
            var refCoupons = mcPriceResult.Details.Map(pi => market.DiscountCurve(pi.Item2.Financing).Zc(pi.Item2.Date));

            var errAbs = Math.Abs(mcCoupons.Sum() - refCoupons.Sum());
            Assert.LessOrEqual(errAbs, 7.0e-5);

            var errRel = Math.Abs(mcCoupons.Sum() / refCoupons.Sum() - 1.0);
            Assert.LessOrEqual(errRel, 8.0e-6);
        }
    }
}
