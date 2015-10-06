using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Product.CouponDsl
{
    public class DslCouponData
    {
        public DslCouponData(DslPayoffExpression expression, PaymentInfo paymentInfo)
        {
            Expression = expression;
            PaymentInfo = paymentInfo;
        }
        public DslPayoffExpression Expression { get; private set; }
        public PaymentInfo PaymentInfo { get; private set; }
         
    }
}