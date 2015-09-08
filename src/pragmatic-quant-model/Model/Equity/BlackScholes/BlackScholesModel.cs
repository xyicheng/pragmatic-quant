using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;

namespace pragmatic_quant_model.Model.Equity.BlackScholes
{
    public class BlackScholesModel : EquityModel
    {
        public BlackScholesModel(ITimeMeasure time, AssetId asset, RrFunction sigma, DiscreteLocalDividend[] dividends)
            : base(asset, dividends, time)
        {
           Sigma = sigma;
        }

        public RrFunction Sigma { get; private set; }
    }

}
