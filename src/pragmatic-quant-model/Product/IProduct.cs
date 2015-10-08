using System;
using System.Linq;
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
                return couponDecomposable.Decomposition().Aggregate(new IFixing[0],
                    (allFixings, cpn) => allFixings.Union(cpn.Accept(this)).ToArray());
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
        }
        #endregion
    }

}
