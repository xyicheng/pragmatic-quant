using System;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Pricing
{
    public static class PriceConverter
    {
        public static Price Convert(this Price price, Currency currency, Market market)
        {
            if (!price.Currency.Equals(currency))
                throw new NotImplementedException("Fx not yet handled");
            return price;
        }
    }
}
