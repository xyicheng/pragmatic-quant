using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Model.BlackScholes
{
    public class BlackScholesModelDescription : IModelDescription
    {
        public BlackScholesModelDescription(string asset, MapRawDatas<DateOrDuration, double> sigma, bool withDivs)
        {
            Sigma = sigma;
            WithDivs = withDivs;
            Asset = asset;
        }
        public string Asset { get; private set; }
        public MapRawDatas<DateOrDuration, double> Sigma { get; private set; }
        public bool WithDivs { get; private set; }
    }

    internal class BlackScholesModelFactory : ModelFactory<BlackScholesModelDescription>
    {
        public static readonly BlackScholesModelFactory Instance = new BlackScholesModelFactory();
        public override IModel Build(BlackScholesModelDescription bs, Market market)
        {
            var time = ModelFactoryUtils.DefaultTime(market.RefDate);

            var assetMkt = market.AssetMarketFromName(bs.Asset);
            var localDividends = bs.WithDivs
                ? assetMkt.Dividends.Map(div => div.DivModel())
                : new DiscreteLocalDividend[0];

            return new BlackScholesModel(time, assetMkt.Asset, bs.Sigma.ToFunction(time), localDividends);
        }
    }
}