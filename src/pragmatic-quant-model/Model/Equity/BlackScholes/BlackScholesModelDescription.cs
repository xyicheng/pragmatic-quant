using System.Diagnostics.Contracts;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Model.Equity.Dividends;

namespace pragmatic_quant_model.Model.Equity.BlackScholes
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

            return new BlackScholesModel(time, assetMkt.Asset, bs.Sigma.ToStepFunction(time), localDividends);
        }
    }
    
    public class BlackScholesModelCalibDesc : ICalibrationDescription
    {
        public BlackScholesModelCalibDesc(string asset, 
                                          bool withDivs,
                                          DateOrDuration[] calibrationMaturities, double[] calibrationStrikes)
        {
            Contract.Requires(calibrationMaturities.Length == calibrationStrikes.Length);
            Asset = asset;
            WithDivs = withDivs;
            CalibrationMaturities = calibrationMaturities;
            CalibrationStrikes = calibrationStrikes; ;
        }
        
        public string Asset { get; private set; }
        public bool WithDivs { get; private set; }
         
        public DateOrDuration[] CalibrationMaturities { get; private set; }
        public double[] CalibrationStrikes { get; private set; }
    }

    public class BlackScholesModelCalibration : ModelCalibration<BlackScholesModelCalibDesc>
    {
        public static readonly BlackScholesModelCalibration Instance = new BlackScholesModelCalibration();
        public override IModelDescription Calibrate(BlackScholesModelCalibDesc bsDesc, Market market)
        {
            AssetMarket assetMkt = market.AssetMarketFromName(bsDesc.Asset);
            var volSurface = assetMkt.VolSurface();
            var forwardCurve = assetMkt.Forward();
            var optionPricer = BlackScholesWithDividendOption.Build(assetMkt.Spot, assetMkt.Dividends, assetMkt.RiskFreeDiscount, assetMkt.Time);
            
            var calibMaturities = bsDesc.CalibrationMaturities.Map(d => d.ToDate(assetMkt.RefDate));
            var calibStrikes = bsDesc.CalibrationStrikes;
            var calibDates = assetMkt.Time[calibMaturities];

            double[] targetPrices = new double[calibDates.Length];
            double[] optionTypes = new double[calibDates.Length];
            for (int i = 0; i < calibMaturities.Length; i++)
            {
                var targetVol = volSurface.Volatility(calibDates[i], calibStrikes[i]);
                var optionType = (calibStrikes[i] > forwardCurve.Fwd(calibMaturities[i])) ? 1.0 : -1.0;
                targetPrices[i] = optionPricer.Price(calibDates[i], calibStrikes[i], targetVol, optionType);
                optionTypes[i] = optionType;
            }

            var calibratedVols = optionPricer.CalibrateVol(calibDates, targetPrices, bsDesc.CalibrationStrikes, optionTypes);
            var sigma = new MapRawDatas<DateOrDuration, double>(bsDesc.CalibrationMaturities, calibratedVols);
            
            return new BlackScholesModelDescription(bsDesc.Asset, sigma, bsDesc.WithDivs);
        }
    } 

}