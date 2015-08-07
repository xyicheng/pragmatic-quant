namespace pragmatic_quant_model.Product.CouponDsl
{
    public class DslCouponData
    {
        public DslCouponData(CouponPayoffExpression expression, PaymentInfo paymentInfo)
        {
            Expression = expression;
            PaymentInfo = paymentInfo;
        }
        public CouponPayoffExpression Expression { get; private set; }
        public PaymentInfo PaymentInfo { get; private set; }
         
    }
}