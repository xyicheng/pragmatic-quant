using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Model;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.Pricing
{
    public interface IPricer
    {
        PriceResult Price(IProduct product, IModel model, Market market);
    }
}