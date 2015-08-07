using System;
using System.Collections.Generic;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Product.CouponDsl;

namespace pragmatic_quant_model.Product
{
    using GenericLegParameters = LabelledMatrix<DateTime, string, object>;

    public static class GenericLegFactory
    {
        public static Leg<DslCoupon> Build(GenericLegParameters legParameters, string dslCouponPayoff)
        {
            string[] paramLabels = legParameters.ColLabels.Map(s => s.ToLowerInvariant().Trim());

            if (paramLabels.Count(l => l == "paycurrency") != 1)
                throw new ArgumentException("Leg Parameters must contain a pay currency !");

            var couponDatas = new List<DslCouponData>();
            for (int row = 0; row < legParameters.RowLabels.Length; row++)
            {
                var paramVals = legParameters.Values.Row(row);
                IDictionary<string, object> couponParameters = paramLabels.ZipToDictionary(paramVals);

                Currency payCurrency = Currency.Parse(couponParameters["paycurrency"].ToString());
                DateTime payDate = legParameters.RowLabels[row];
                var couponPayment = new PaymentInfo(payCurrency, payDate);

                var payoff = CouponPayoffExpressionParser.Parse(dslCouponPayoff, couponParameters);
                var dslCouponData = new DslCouponData(payoff, couponPayment);

                couponDatas.Add(dslCouponData);
            }

            var coupons = DslCouponCompiler.BuildCoupon(couponDatas.ToArray());
            return new Leg<DslCoupon>(coupons);
        }
    }
}