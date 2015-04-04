using System;
using pragmatic_quant_model.Market;

namespace pragmatic_quant_model.Factories
{
    public class RateMarketFactory : IFactoryFromBag<RateMarket>
    {
        public RateMarket Build(object[,] bag)
        {
            var curveRawDatas = BagServices.ProcessTimeMatrixDatas(bag, "Discount");

            throw new NotImplementedException();
        }
    }
}
