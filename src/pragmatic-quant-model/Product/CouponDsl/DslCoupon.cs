using System;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Product.CouponDsl
{
    public class DslCoupon : Coupon
    {
        #region private fields
        private readonly Func<object, double[], double> payoff;
        private readonly object payoffObj;
        #endregion
        public DslCoupon(PaymentInfo paymentInfo, IFixing[] fixings,
                         Func<object, double[], double> payoff, object payoffObj)
            : base(paymentInfo, fixings)
        {
            this.payoff = payoff;
            this.payoffObj = payoffObj;
        }

        public override double Payoff(double[] fixings)
        {
            return payoff(payoffObj, fixings);
        }
    }
}