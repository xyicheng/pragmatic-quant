using System;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Model
{
    public interface IFactorRepresentationFactory
    {
        IFactorModelRepresentation Build(IModel model, Market market);
    }

    public abstract class FactorRepresentationFactory<TModel>
        : IFactorRepresentationFactory where TModel : class, IModel
    {
        protected abstract IFactorModelRepresentation Build(TModel model, Market market);
        public IFactorModelRepresentation Build(IModel model, Market market)
        {
            var modelImplem = model as TModel;
            if (modelImplem == null)
                throw new Exception(string.Format("FactorRepresentationFactory : {0} expected but was {1}", typeof (TModel), model.GetType()));
            return Build((TModel) model, market);
        }
    }
}