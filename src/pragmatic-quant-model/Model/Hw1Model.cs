using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;

namespace pragmatic_quant_model.Model
{
    public class Hw1Model : IModel
    {
        public Hw1Model(ITimeMeasure time, Currency currency, double meanReversion, RrFunction sigma)
        {
            Sigma = sigma;
            MeanReversion = meanReversion;
            Currency = currency;
            Time = time;
        }

        public Currency Currency { get; private set; }
        public double MeanReversion { get; private set; }
        public RrFunction Sigma { get; private set; }

        public ITimeMeasure Time { get; private set; }
    }
}