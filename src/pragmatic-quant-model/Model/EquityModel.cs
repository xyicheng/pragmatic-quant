using System;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.Model
{
    public abstract class EquityModel : IModel
    {
        protected EquityModel(AssetId asset, ITimeMeasure time)
        {
            Asset = asset;
            Time = time;
        }

        public AssetId Asset { get; private set; }
        public ITimeMeasure Time { get; private set; }
        public Currency PivotCurrency { get { return Asset.Currency; } }
        
    }

    public class EquityFactorRepresentationFactory : FactorRepresentationFactory<EquityModel>
    {
        public static readonly EquityFactorRepresentationFactory Instance = new EquityFactorRepresentationFactory();
        protected override IFactorModelRepresentation Build(EquityModel model, Market market, PaymentInfo probaMeasure)
        {
            var asset = model.Asset;

            if (!probaMeasure.Financing.Currency.Equals(probaMeasure.Currency)
                || !probaMeasure.Currency.Equals(asset.Currency))
                throw new NotImplementedException("TODO !");

            var numeraireDiscount = market.DiscountCurve(probaMeasure.Financing);
            var assetMkt = market.AssetMarket(asset);
            var assetDiscountCurve = assetMkt.AssetFinancingCurve(numeraireDiscount);

            var zc = new DeterministicZcRepresentation(model.PivotCurrency, 1);
            var bsSpot = new EquitySpotRepresentation(model.Asset, assetDiscountCurve, probaMeasure);
            return new FactorRepresentation(market, new[] { zc }, new[] { bsSpot });
        }
    }

}