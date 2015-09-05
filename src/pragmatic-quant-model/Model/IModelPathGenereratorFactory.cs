﻿using System;
using System.Collections.Generic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths.Stochastic;
using pragmatic_quant_model.Model.BlackScholes;
using pragmatic_quant_model.Model.HullWhite;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.Model
{
    public interface IModelPathGenereratorFactory
    {
        IProcessPathGenerator Build(IModel model, Market market, 
                                    PaymentInfo probaMeasure, DateTime[] simulatedDates);
    }

    public class ModelPathGeneratorFactories
    {
        #region private fields
        private static readonly IDictionary<Type, IModelPathGenereratorFactory> factories = GetFactories();
        #endregion
        #region private methods
        private static IDictionary<Type, IModelPathGenereratorFactory> GetFactories()
        {
            var result = new Dictionary<Type, IModelPathGenereratorFactory>
            {
                {typeof (Hw1ModelDescription), Hw1ModelPathGeneratorFactory.Instance},
                {typeof (BlackScholesModelDescription), BlackScholesEqtyPathGeneratorFactory.Instance}
            };
            return result;
        }
        #endregion

        public static IModelPathGenereratorFactory For(IModelDescription model)
        {
            IModelPathGenereratorFactory modelPathGenfactory;
            if (factories.TryGetValue(model.GetType(), out modelPathGenfactory))
                return modelPathGenfactory;
            throw new ArgumentException(string.Format("Missing ModelPathGeneratorFactory for {0}", model));
        }
    }
    
    public abstract class ModelPathGenereratorFactory<TModel> : IModelPathGenereratorFactory where TModel : class, IModel
    {
        protected abstract IProcessPathGenerator Build(TModel model, Market market, PaymentInfo probaMeasure, DateTime[] simulatedDates);
        
        public IProcessPathGenerator Build(IModel model, Market market, 
                                           PaymentInfo probaMeasure, DateTime[] simulatedDates)
        {
            var modelImplem = model as TModel;
            if (modelImplem == null)
                throw new Exception(string.Format("ModelPathGenereratorFactory : {0} expected but was {1}",
                                                    typeof (TModel), model.GetType()));
            return Build(modelImplem, market, probaMeasure, simulatedDates);
        }
    }
}