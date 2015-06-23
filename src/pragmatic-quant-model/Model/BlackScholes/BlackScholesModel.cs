using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;

namespace pragmatic_quant_model.Model.BlackScholes
{
    public class BlackScholesModel : IModel
    {
        public BlackScholesModel(ITimeMeasure time, AssetId asset, RrFunction sigma, DiscreteLocalDividend[] dividends)
        {
            Dividends = dividends;
            Sigma = sigma;
            Asset = asset;
            Time = time;
        }

        public AssetId Asset { get; private set; }
        public RrFunction Sigma { get; private set; }
        public DiscreteLocalDividend[] Dividends { get; private set; }

        public ITimeMeasure Time { get; private set; }
        public Currency PivotCurrency { get { return Asset.Currency; } }
    }
}
