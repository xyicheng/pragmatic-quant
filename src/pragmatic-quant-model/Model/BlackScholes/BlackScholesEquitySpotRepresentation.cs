using System;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.Model.BlackScholes
{
    public class BlackScholesEquitySpotRepresentation : EquitySpotRepresentation
    {
        #region private fields
        private readonly DiscountCurve assetDiscountCurve;
        private readonly PaymentInfo probaMeasure;
        #endregion
        public BlackScholesEquitySpotRepresentation(BlackScholesModel bsModel, DiscountCurve assetDiscountCurve, PaymentInfo probaMeasure)
            : base(bsModel.Asset)
        {
            this.assetDiscountCurve = assetDiscountCurve;
            this.probaMeasure = probaMeasure;
        }
        public override RnRFunction Spot(DateTime date, double initialSpot)
        {
            var forwardWithoutDivs = assetDiscountCurve.Zc(probaMeasure.Date) / assetDiscountCurve.Zc(date);
            return RnRFunctions.Affine(0.0, new[] {forwardWithoutDivs});
        }
    }

    public class BlackScholesFactorRepresentationFactory : FactorRepresentationFactory<BlackScholesModel>
    {
        public static readonly BlackScholesFactorRepresentationFactory Instance = new BlackScholesFactorRepresentationFactory();
        protected override IFactorModelRepresentation Build(BlackScholesModel model, Market market, PaymentInfo probaMeasure)
        {
            var asset = model.Asset;

            if (!probaMeasure.Financing.Currency.Equals(probaMeasure.Currency)
                || !probaMeasure.Currency.Equals(asset.Currency))
                throw new NotImplementedException("TODO !");

            var numeraireDiscount = market.DiscountCurve(probaMeasure.Financing);
            var assetMkt = market.AssetMarket(asset);
            var assetDiscountCurve = assetMkt.AssetFinancingCurve(numeraireDiscount);

            var zc = new DeterministicZcRepresentation(model.PivotCurrency, 1);
            var bsSpot = new BlackScholesEquitySpotRepresentation(model, assetDiscountCurve, probaMeasure);
            return new FactorRepresentation(market, new[] {zc}, new[] {bsSpot});
        }
    }
}