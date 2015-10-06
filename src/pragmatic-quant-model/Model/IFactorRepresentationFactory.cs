using System;
using System.Collections.Generic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Model.Equity;
using pragmatic_quant_model.Model.Equity.BlackScholes;
using pragmatic_quant_model.Model.Equity.LocalVolatility;
using pragmatic_quant_model.Model.HullWhite;

namespace pragmatic_quant_model.Model
{
    public interface IFactorRepresentationFactory
    {
        IFactorModelRepresentation Build(IModel model, Market market, PaymentInfo probaMeasure);
    }

    public static class FactorRepresentationFactories
    {
        #region private fields
        private static readonly IDictionary<Type, IFactorRepresentationFactory> factories = GetFactories();
        #endregion
        #region private methods
        private static IDictionary<Type, IFactorRepresentationFactory> GetFactories()
        {
            var result = new Dictionary<Type, IFactorRepresentationFactory>
            {
                {typeof (Hw1Model), Hw1FactorRepresentationFactory.Instance},
                {typeof (BlackScholesModel), EquityFactorRepresentationFactory.Instance},
                {typeof (LocalVolatilityModel), EquityFactorRepresentationFactory.Instance}
            };
            return result;
        }
        #endregion

        public static IFactorRepresentationFactory For(IModel model)
        {
            IFactorRepresentationFactory factorRepresentationFactory;
            if (factories.TryGetValue(model.GetType(), out factorRepresentationFactory))
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