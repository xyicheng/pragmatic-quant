using System.Collections.Generic;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.Product
{
    public class PriceResult
    {
        public PriceResult(Price price, IDictionary<PaymentInfo, Price> details)
        {
            Details = details;
            Price = price;
        }
        public Price Price { get; private set; }
        public IDictionary<PaymentInfo, Price> Details { get; private set; }
    }
}