using System;
using System.Linq;
using System.Collections.Generic;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths.Function;
using pragmatic_quant_model.Model.Equity;
using pragmatic_quant_model.Model.Equity.BlackScholes;
using pragmatic_quant_model.Model.Equity.LocalVolatility;
using pragmatic_quant_model.Model.HullWhite;

namespace pragmatic_quant_model.Model
{
    public interface IModelDescription { }

    public interface IModelFactory
    {
        IModel Build(IModelDescription modelDescription, Market market);
    }

    public class ModelFactory : Singleton<ModelFactory>, IModelFactory
    {
        #region private fields
        private static readonly IDictionary<Type, IModelFactory> factories = GetFactories();
        #endregion
        #region private methods
        private static IDictionary<Type, IModelFactory> GetFactories()
        {
            var result = new Dictionary<Type, IModelFactory>
            {
                {typeof (Hw1ModelDescription), Hw1ModelFactory.Instance},
                {typeof (BlackScholesModelDescription), BlackScholesModelFactory.Instance},
                {typeof (LocalVolModelDescription), LocalVolModelFactory.Instance}
            };
            return result;
        }
        #endregion

        public IModel Build(IModelDescription modelDescription, Market market)
        {
            IModelFactory modelfactory;
            if (factories.TryGetValue(modelDescription.GetType(), out modelfactory))
                return modelfactory.Build(modelDescription, market);
            throw new ArgumentException(string.Format("Missing Model Factory for {0}", modelDescription));
        }
    }

    internal abstract class ModelFactory<TModelDesc> : IModelFactory
        where TModelDesc : class, IModelDescription
    {
        public abstract IModel Build(TModelDesc model, Market market);
        public IModel Build(IModelDescription modelDescription, Market market)
        {
            var modelDescImplem = modelDescription as TModelDesc;
            if (modelDescImplem == null)
                throw new Exception(string.Format("ModelFactory : {0} expected but was {1}",
                                                  typeof (TModelDesc), modelDescription.GetType()));
            return Build(modelDescImplem, market);
        }
    }
    
    internal static class ModelFactoryUtils
    {
        public static ITimeMeasure DefaultTime(DateTime refDate)
        {
            return TimeMeasure.Act365(refDate);
        }
        public static RrFunction ToFunction(this MapRawDatas<DateOrDuration, double> rawDatasFunc, ITimeMeasure time)
        {
            var abscissae = rawDatasFunc.Pillars.Map(d => time[d.ToDate(time.RefDate)]);
            return new StepFunction(abscissae, rawDatasFunc.Values, rawDatasFunc.Values.First());
        }
        public static DiscreteLocalDividend DivModel(this DividendQuote divQuote)
        {
            return DiscreteLocalDividend.AffineDividend(divQuote.Date, divQuote.Cash, divQuote.Yield);
        }
    }

}