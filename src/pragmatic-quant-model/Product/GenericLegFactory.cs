using System;
using System.Collections.Generic;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Product.CouponDsl;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_model.Product
{
    using GenericLegParameters = LabelledMatrix<DateTime, string, object>;

    public static class GenericLegFactory
    {
        public static IFixingFunction[] BuildDslFixingFunctions(GenericLegParameters legParameters, string[] dslCouponPayoffs)
        {
            string[] paramLabels = legParameters.ColLabels.Map(s => s.ToLowerInvariant().Trim());
            
            var couponDatas = new DslPayoffExpression[legParameters.RowLabels.Length];
            for (int row = 0; row < couponDatas.Length; row++)
            {
                object[] paramVals = legParameters.Values.Row(row);
                IDictionary<string, object> couponParameters = paramLabels.ZipToDictionary(paramVals);
                var payoff = DslPayoffParser.Parse(dslCouponPayoffs[row], couponParameters);
                couponDatas[row] = payoff;
            }

            return DslPayoffCompiler.Compile(couponDatas);
        }

        public static Coupon[] BuildDslCoupons(string legId, GenericLegParameters legParameters, string[] dslCouponPayoffs)
        {
            var payCurencyLabels = legParameters.ColLabels
                .Where(l => l.Equals(legId + "PayCurrency", StringComparison.InvariantCultureIgnoreCase))
                .ToArray();
            
            if (!payCurencyLabels.Any())
                throw new Exception(string.Format("Missing {0} parameter for building leg", legId + "PayCurrency"));
            
            if (payCurencyLabels.Count()!=1)
                throw new Exception(string.Format("Multiple {0} parameters !", legId + "PayCurrency"));
            
            var payCurrencies = legParameters.GetCol(payCurencyLabels.First()).Map(o => Currency.Parse(o.ToString()));
            var payDates = legParameters.RowLabels;
            var couponPayments = payDates.ZipWith(payCurrencies, (d, c) => new PaymentInfo(c, d));

            var couponPayoffs = BuildDslFixingFunctions(legParameters, dslCouponPayoffs);
            return couponPayments.ZipWith(couponPayoffs, (payInfo, payoff) => new Coupon(payInfo, payoff));
        }
    }
}