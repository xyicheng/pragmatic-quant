using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Model.BlackScholes;

namespace pragmatic_quant_model.Model.LocalVolatility
{
    public class LocalVolatilityModel : EquityModel
    {
        public LocalVolatilityModel(ITimeMeasure time, AssetId asset, LocalVariance localVariance,  MoneynessProvider moneyness, DiscreteLocalDividend[] dividends)
            : base(asset, time)
        {
            LocalVariance = localVariance;
            Dividends = dividends;
            Moneyness = moneyness;
        }

        public LocalVariance LocalVariance { get; private set; }
        public MoneynessProvider Moneyness { get; private set; }
        public DiscreteLocalDividend[] Dividends { get; private set; }
    }

    public class LocalVolModelDescription : IModelDescription
    {
        public LocalVolModelDescription(string asset, bool withDivs, VolatilityMatrix volMatrix)
        {
            WithDivs = withDivs;
            VolMatrix = volMatrix;
            Asset = asset;
        }
        public string Asset { get; private set; }
        public bool WithDivs { get; private set; }
        public VolatilityMatrix VolMatrix { get; private set; }
    }

    internal class LocalVolModelFactory : ModelFactory<LocalVolModelDescription>
    {
        public static readonly LocalVolModelFactory Instance = new LocalVolModelFactory();
        public override IModel Build(LocalVolModelDescription lv, Market market)
        {
            AssetMarket assetMkt = market.AssetMarketFromName(lv.Asset);
           
            DiscreteLocalDividend[] localDividends = lv.WithDivs
                ? assetMkt.Dividends.Map(div => div.DivModel())
                : new DiscreteLocalDividend[0];
            
            AssetForwardCurve assetForward = assetMkt.Forward();
            MoneynessProvider moneyness = MoneynessProvider.FromFwdCurve(assetForward);
            VolatilitySurface volSurface = VolatilitySurface.BuildInterpol(lv.VolMatrix, moneyness);

            return new LocalVolatilityModel(lv.VolMatrix.Time, assetMkt.Asset, volSurface.LocalVariance, moneyness, localDividends);
        }
    }

    public class LocalVolModelCalibDesc : ICalibrationDescription
    {
        public LocalVolModelCalibDesc(string asset, bool withDivs)
        {
            Asset = asset;
            WithDivs = withDivs;
        }
        public string Asset { get; private set; }
        public bool WithDivs { get; private set; } 
    }

    public class LocalVolModelCalibration : ModelCalibration<LocalVolModelCalibDesc>
    {
        public static readonly LocalVolModelCalibration Instance = new LocalVolModelCalibration();
        public override IModelDescription Calibrate(LocalVolModelCalibDesc lvDesc, Market market)
        {
            AssetMarket assetMkt = market.AssetMarketFromName(lvDesc.Asset);
            return new LocalVolModelDescription(lvDesc.Asset, lvDesc.WithDivs, assetMkt.VolMatrix);
        }
    }
}