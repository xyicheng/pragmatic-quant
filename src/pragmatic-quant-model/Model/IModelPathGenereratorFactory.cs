using System;
using pragmatic_quant_model.Maths.Stochastic;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.Model
{
    public interface IModelPathGenereratorFactory
    {
        IProcessPathGenerator Build(IModel model, PaymentInfo probaMeasure, DateTime[] simulatedDates);
    }

    public abstract class ModelPathGenereratorFactory<TModel> : IModelPathGenereratorFactory where TModel : class, IModel
    {
        protected abstract IProcessPathGenerator Build(TModel model, PaymentInfo probaMeasure, DateTime[] simulatedDates);
        public IProcessPathGenerator Build(IModel model, PaymentInfo probaMeasure, DateTime[] simulatedDates)
        {
            var modelImplem = model as TModel;
            if (modelImplem == null)
                throw new Exception(string.Format("ModelPathGenereratorFactory : {0} expected but was {1}", typeof(TModel), model.GetType()));
            return Build(modelImplem, probaMeasure, simulatedDates);
        }
    }
}