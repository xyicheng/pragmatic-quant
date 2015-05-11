using System;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.Model
{
    public interface IFactorModelRepresentation
    {
        Func<double[], double> this[IObservation observation] { get; }
    }


    public abstract class RateZcRepresentation
    {
        protected RateZcRepresentation(Currency currency)
        {
            Currency = currency;
        }
        public abstract RnRFunction Zc(DateTime start, DateTime end);
        public Currency Currency { get; private set; }
    }
}