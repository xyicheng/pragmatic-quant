using System.Diagnostics;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Basic
{
    [DebuggerDisplay("Price = {Value} {Currency}")]
    public class Price
    {
        public Price(double value, Currency currency)
        {
            Currency = currency;
            Value = value;
        }
        public double Value { get; private set; }
        public Currency Currency { get; private set; }
    }
}
