using System;
using System.Collections.Generic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Model.BlackScholes;
using pragmatic_quant_model.Model.HullWhite;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.Model
{
    public interface IFactorRepresentationFactory
    {
        IFactorModelRepresentation Build(IModel model, Market market, PaymentInfo probaMeasure);
    }

    public static class FactorRepresentationFactories
    {
        #region private fields
        private static readonly IDictionary<Type, IFactorRepresentationFactory> Factories = GetFactories();
        #endregion
        #region private methods
        private static IDictionary<Type, IFactorRepresentationFactory> GetFactories()
        {
            var result = new Dictionary<Type, IFactorRepresentationFactory>
            {
                {typeof (Hw1ModelDescription), Hw1FactorRepresentationFactory.Instance},
                {typeof (BlackScholesModelDescription), BlackScholesFactorRepresentationFactory.Instance}
            };
            return result;
        }
        #endregion

        public static IFactorRepresentationFactory For(IModelDescription model)
        {
            IFactorRepresentationFactory factorRepresentationFactory;
            if (Factories.TryGetValue(model.GetType(), out factorRepresentationFactory))
                return factorRepresentationFactory;
            throw new ArgumentException(string.Format("Missing Factor Representation Factory for {0}", model));
        }
    }
    
    public abstract class FactorRepresentationFactory<TModel>
        : IFactorRepresentationFactory where TModel : class, IModel
    {
        protected abstract IFactorModelRepresentation Build(TModel model, Market market, PaymentInfo probaMeasure);
        public IFactorModelRepresentation Build(IModel model, Market market, PaymentInfo probaMeasure)
        {
            var modelImplem = model as TModel;
            if (modelImplem == null)
                throw new Exception(string.Format("FactorRepresentationFactory : {0} expected but was {1}", 
                                                                                typeof (TModel), model.GetType()));
            return Build(modelImplem, market, probaMeasure);
        }
    }
}