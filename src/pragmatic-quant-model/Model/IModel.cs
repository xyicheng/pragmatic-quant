using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;

namespace pragmatic_quant_model.Model
{
    public interface IModel
    {
        ITimeMeasure Time { get; }
        Currency PivotCurrency { get; }
    }
}
