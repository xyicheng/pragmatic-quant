using System;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_model.Product
{
    public interface ICallableProduct : IProduct
    {
        DateTime[] CallDates { get; }
        ICouponDecomposable Underlying { get; }
        ICouponDecomposable Redemption(DateTime callDate);
    }

    public class AutoCall : ICallableProduct
    {
        public FinancingId Financing { get; private set; }
        public TResult Accept<TResult>(IProductVisitor<TResult> visitor)
        {
            throw new NotImplementedException();
        }

        public DateTime[] CallDates { get; private set; }
        public ICouponDecomposable Underlying
        {
            get { throw new NotImplementedException(); }
        }
        public ICouponDecomposable Redemption(DateTime callDate)
        {
            throw new NotImplementedException();
        }
        public IFixingFunction CallTrigger(DateTime callDate)
        {
            throw new NotImplementedException();
        }
    }
}