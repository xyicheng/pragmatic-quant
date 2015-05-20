using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Model
{
    public interface IModel
    {
        ITimeMeasure Time { get; }
        Currency PivotCurrency { get; }
    }
}
