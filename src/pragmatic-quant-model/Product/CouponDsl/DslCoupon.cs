using System.Collections.Generic;
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

        public static Coupon Parse(PaymentInfo paymentInfo, IDictionary<string, object> couponParameters, string dslPayoffScript)
        {
            CouponPayoffExpression payoff = CouponPayoffExpressionParser.Parse(dslPayoffScript, couponParameters);
            return DslCouponCompiler.BuildCoupon(paymentInfo, payoff);
        }

        public override double Payoff(double[] fixings)
        {
            return (double) payoff.Invoke(null, new object[] {fixings});
        }
    }
}