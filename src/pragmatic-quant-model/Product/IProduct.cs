using System;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_model.Product
{
    public interface IProduct
    {
        FinancingId Financing { get; }

        TResult Accept<TResult>(IProductVisitor<TResult> visitor);
    }

    public interface IProductVisitor<out TResult>
    {
        TResult Visit(Coupon coupon);
        TResult Visit(ICouponDecomposable couponDecomposable);
        TResult Visit(AutoCall autocall);
    }

    public static class ProductUtils
    {
        public static IFixing[] RetrieveFixings(this IProduct product)
        {
            return product.Accept(new FixingProductVisitor());
        }
        public static DateTime[] RetrieveEventDates(this IProduct product)
        {
            return product.Accept(new EventDateProductVisitor());
        }
        public static PaymentInfo[] RetrievePaymentInfos(this IProduct product)
        {
            return product.Accept(new PaymentProductVisitor());
        }

        #region private methods
        private class FixingProductVisitor : IProductVisitor<IFixing[]>
        {
            public IFixing[] Visit(Coupon coupon)
            {
                return coupon.Fixings;
            }
            public IFixing[] Visit(ICouponDecomposable couponDecomposable)
            {
                var fixings = EnumerableUtils.Merge(couponDecomposable.Decomposition().Map(Visit));
                return fixings.OrderBy(f => f.Date).ToArray();
            }
            public IFixing[] Visit(AutoCall autocall)
            {
                var underlyingFixings = autocall.Underlying.Accept(this);
                var redemptionFixings = autocall.CallDates.Map(d => autocall.Redemption(d).Accept(this));
                var triggerFixings = autocall.CallDates.Map(d=> autocall.CallTrigger(d).Fixings);

                var mergedFixings = underlyingFixings.MergeWith(redemptionFixings)
                                                     .MergeWith(triggerFixings);
                return mergedFixings.OrderBy(f => f.Date).ToArray();
            }
        }

        private class EventDateProductVisitor : IProductVisitor<DateTime[]>
        {
            public DateTime[] Visit(Coupon coupon)
            {
                var fixingsDates = coupon.Fixings.Select(f => f.Date).Distinct();
                return fixingsDates.Union(new[] {coupon.PaymentInfo.Date}).ToArray();
            }
            public DateTime[] Visit(ICouponDecomposable couponDecomposable) 
            {
                return couponDecomposable.Decomposition().Aggregate(new DateTime[0],
                    (allEventDates, cpn) => allEventDates.Union(cpn.Accept(this)).ToArray());
            }
            public DateTime[] Visit(AutoCall autocall)
            {
                var underlyingDates = autocall.Underlying.Accept(this);
                var redemptionDates = autocall.CallDates.Map(d => autocall.Redemption(d).Accept(this));

                var result = EnumerableUtils.Merge(redemptionDates);
                result = EnumerableUtils.Merge(underlyingDates, result);
                result = EnumerableUtils.Merge(result, autocall.CallDates);
                result = result.OrderBy(d => d).ToArray();
                return result;
            }
        }

        private class PaymentProductVisitor : IProductVisitor<PaymentInfo[]>
        {
            public PaymentInfo[] Visit(Coupon coupon)
            {
                return new[] {coupon.PaymentInfo};
            }
            public PaymentInfo[] Visit(ICouponDecomposable couponDecomposable)
            {
                return couponDecomposable.Decomposition().Aggregate(new PaymentInfo[0],
                    (allPayments, cpn) => allPayments.Union(cpn.Accept(this)).ToArray());
            }
            public PaymentInfo[] Visit(AutoCall autocall)
            {
                var underlyingPayments = autocall.Underlying.Accept(this);
                var redemptionPayments = autocall.CallDates.Map(d => autocall.Redemption(d).Accept(this));
                return redemptionPayments.Aggregate(underlyingPayments, (prev, d) => prev.Union(d).ToArray());
            }
        }
        #endregion
    }

}
