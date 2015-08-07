using System.Reflection;

namespace pragmatic_quant_model.Product.CouponDsl
{
    public class DslCoupon : Coupon
    {
        #region private fields
        private readonly MethodInfo payoff;
        #endregion
        public DslCoupon(PaymentInfo paymentInfo, IFixing[] fixings, MethodInfo payoff)
            : base(paymentInfo, fixings)
        {
            this.payoff = payoff;
        }

        public override double Payoff(double[] fixings)
        {
            return (double) payoff.Invoke(null, new object[] {fixings});
        }
    }
}