using System;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Model
{
    public interface IModelDescription { }

    public class Hw1ModelDescription : IModelDescription
    {
        public Hw1ModelDescription(Currency currency, double meanReversion, MapRawDatas<DateTime, double> sigma)
        {
            Sigma = sigma;
            MeanReversion = meanReversion;
            Currency = currency;
        }
        public Currency Currency { get; private set; }
        public double MeanReversion { get; private set; }
        public MapRawDatas<DateTime, double> Sigma { get; private set; }
    }
}