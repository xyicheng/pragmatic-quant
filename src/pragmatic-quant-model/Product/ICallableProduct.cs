using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_model.Product
{
    public interface ICallableProduct : IProduct
    {
        ICouponDecomposable Underlying { get; }
        DateTime[] CallDates { get; }
        ICouponDecomposable Redemption(DateTime callDate);
    }

    public class AutoCall : ICallableProduct
    {
        #region private fields
        private readonly IDictionary<DateTime, ICouponDecomposable> redemptions;
        private readonly IDictionary<DateTime, IFixingFunction> triggers;
        #endregion
        public AutoCall(ICouponDecomposable underlying, DateTime[] callDates,
                        ICouponDecomposable[] redemptionCoupons, IFixingFunction[] triggers)
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
        public ICouponDecomposable Redemption(DateTime callDate)
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
            throw new NotImplementedException();
        }
    }
}