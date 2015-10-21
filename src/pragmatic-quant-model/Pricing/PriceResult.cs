using System;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Pricing
{
    public interface IPricingResult
    {
    }
    
    public class PriceResult : IPricingResult
    {
        public PriceResult(Price price, Tuple<string, PaymentInfo, Price>[] details)
        {
            Details = details;
            Price = price;
        }
        public Price Price { get; private set; }
        public Tuple<string, PaymentInfo, Price>[] Details { get; private set; }
    }
}