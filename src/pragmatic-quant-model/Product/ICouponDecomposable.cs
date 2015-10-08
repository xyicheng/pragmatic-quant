using System.Linq;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Product
{
    public interface ICouponDecomposable : IProduct
    {
        Coupon[] Decomposition();
    }

    public class Leg<TCoupon> : ICouponDecomposable 
        where TCoupon : Coupon
    {
        #region private fields
        private readonly TCoupon[] coupons;
        #endregion
        public Leg(TCoupon[] coupons)
        {
            this.coupons = coupons;
        }
        
        public FinancingId Financing
        {
            get { return coupons.Select(cpn => cpn.Financing).Distinct().Single(); }
        }
        public TResult Accept<TResult>(IProductVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
        public Coupon[] Decomposition()
        {
            return coupons.Cast<Coupon>().ToArray();
        }
    }
}