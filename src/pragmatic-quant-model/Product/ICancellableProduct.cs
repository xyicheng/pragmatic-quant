using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_model.Product
{
    public interface ICancellableProduct : IProduct
    {
        ICouponDecomposable Underlying { get; }
        DateTime[] CallDates { get; }
        Coupon Redemption(DateTime callDate);
    }

    public class AutoCall : ICancellableProduct
    {
        #region private fields
        private readonly IDictionary<DateTime, Coupon> redemptions;
        private readonly IDictionary<DateTime, IFixingFunction> triggers;
        #endregion
        public AutoCall(ICouponDecomposable underlying, DateTime[] callDates,
                        Coupon[] redemptionCoupons, IFixingFunction[] triggers)
        {
            Contract.ForAll(redemptionCoupons, c => c.Financing.Equals(underlying.Financing));
            Underlying = underlying;
            Financing = underlying.Financing;
            CallDates = callDates;
            redemptions = callDates.ZipToDictionary(redemptionCoupons);
            this.triggers = callDates.ZipToDictionary(triggers);
        }

        public ICouponDecomposable Underlying { get; private set; }
        public DateTime[] CallDates { get; private set; }
        public Coupon Redemption(DateTime callDate)
        {
            return redemptions[callDate];
        }
        public IFixingFunction CallTrigger(DateTime callDate)
        {
            return triggers[callDate];
        }

        public FinancingId Financing { get; private set; }
        public TResult Accept<TResult>(IProductVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public static class CallableUtils
    {
        /// <summary>
        /// Return underlying coupons whose paydate are within ]callDate, next call date ] 
        /// </summary>
        public static Coupon[] NextUnderlyingCoupons(this ICancellableProduct cancellable, DateTime callDate)
        {
            var callDateIndex = Array.BinarySearch(cancellable.CallDates, callDate);
            var nextCallDate = (callDateIndex == cancellable.CallDates.Length - 1)
                ? DateTime.MaxValue
                : cancellable.CallDates[callDateIndex + 1];
            var underlyingCoupons = cancellable.Underlying.Decomposition();
            var result = underlyingCoupons.Where(cpn =>
            {
                var payDate = cpn.PaymentInfo.Date;
                return payDate > callDate && payDate <= nextCallDate;
            }).ToArray();
            return result;
        }

    }
}