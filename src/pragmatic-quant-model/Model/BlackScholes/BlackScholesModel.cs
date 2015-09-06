using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;

namespace pragmatic_quant_model.Model.BlackScholes
{
    public class BlackScholesModel : EquityModel
    {
        public BlackScholesModel(ITimeMeasure time, AssetId asset, RrFunction sigma, DiscreteLocalDividend[] dividends)
            :base(asset, time)
        {
            Dividends = dividends;
            Sigma = sigma;
        }

        public RrFunction Sigma { get; private set; }
        public DiscreteLocalDividend[] Dividends { get; private set; }
    }

}
