using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Model
{
    public interface IFactorRepresentationFactory<in TModel> where TModel : IModel
    {
        IFactorModelRepresentation Build(TModel model, Market market);
    }
}