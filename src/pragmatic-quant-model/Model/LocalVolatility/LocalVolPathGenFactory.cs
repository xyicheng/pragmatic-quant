using System;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths.Stochastic;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.Model.LocalVolatility
{
    public class LocalVolPathGenFactory : ModelPathGenereratorFactory<LocalVolatilityModel>
    {
        public static readonly LocalVolPathGenFactory Instance = new LocalVolPathGenFactory();
        protected override IProcessPathGenerator Build(LocalVolatilityModel model, Market market, PaymentInfo probaMeasure, DateTime[] simulatedDates)
        {
            throw new NotImplementedException();
        }
    }
}