using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Model.BlackScholes;

namespace pragmatic_quant_model.Model.LocalVolatility
{
    public class LocalVolatilityModel : IModel
    {
        public LocalVolatilityModel(ITimeMeasure time, AssetId asset, LocalVariance localVariance, DiscreteLocalDividend[] dividends)
        {
            Time = time;
            Asset = asset;
            LocalVariance = localVariance;
            Dividends = dividends;
        }

        public AssetId Asset { get; private set; }
        public LocalVariance LocalVariance { get; private set; }
        public DiscreteLocalDividend[] Dividends { get; private set; }

        public ITimeMeasure Time { get; private set; }
        public Currency PivotCurrency
        {
            get { return Asset.Currency; }
        }
    }
}