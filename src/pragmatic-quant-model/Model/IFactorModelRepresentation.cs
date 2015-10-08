using System;
using System.Collections.Generic;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_model.Model
{
    public interface IFactorModelRepresentation
    {
        Func<double[], double> this[IObservation observation] { get; }
    }

    public class FactorRepresentation : IFactorModelRepresentation
    {
        #region private fields
        private readonly Market market;
        private readonly IDictionary<Currency, RateZcRepresentation> zcRepresentations;
        private readonly IDictionary<AssetId, EquitySpotRepresentation> equityRepresentations;
        #endregion
        #region private methods
        private Func<double[], double> Zc(Zc zc)
        {
            RateZcRepresentation zcRepresentation;
            if (zcRepresentations.TryGetValue(zc.Currency, out zcRepresentation))
            {
                var discountCurve = market.DiscountCurve(zc.FinancingId);
                var fwdZc = discountCurve.Zc(zc.PayDate) / discountCurve.Zc(zc.Date);
                return zcRepresentation.Zc(zc.Date, zc.PayDate, fwdZc).Eval;
            }
            throw new Exception(string.Format("Not handled currency {0}", zc.Currency));
        }
        private Func<double[], double> EquitySpot(EquitySpot eqtySpot)
        {
            EquitySpotRepresentation equityRepresentation;
            if (equityRepresentations.TryGetValue(eqtySpot.AssetId, out equityRepresentation))
            {
                return equityRepresentation.Spot(eqtySpot.Date).Eval;
            }
            throw new Exception(string.Format("Not handled equity {0}", eqtySpot.AssetId));
        }
        #endregion
        public FactorRepresentation(Market market,
                                    IEnumerable<RateZcRepresentation> zcs,
                                    IEnumerable<EquitySpotRepresentation> equities)
        {
            this.market = market;
            zcRepresentations = zcs.ToDictionary(zcRep => zcRep.Currency, zcRep => zcRep);
            equityRepresentations = equities.ToDictionary(eqty => eqty.Asset, eqty => eqty);
        }
        public FactorRepresentation(Market market, params RateZcRepresentation[] zcs)
            : this(market, zcs, new EquitySpotRepresentation[0]) { }

        public Func<double[], double> this[IObservation observation]
        {
            get
            {
                var zc = observation as Zc;
                if (zc != null) return Zc(zc);
                
                var eqtySpot = observation as EquitySpot;
                if (eqtySpot != null) return EquitySpot(eqtySpot);

                throw new NotImplementedException(observation.ToString());
            }
        }
    }

    public abstract class RateZcRepresentation
    {
        protected RateZcRepresentation(Currency currency)
        {
            Currency = currency;
        }
        public abstract RnRFunction Zc(DateTime date, DateTime maturity, double fwdZc);
        public Currency Currency { get; private set; }
    }

    public class DeterministicZcRepresentation : RateZcRepresentation
    {
        #region private fields
        private readonly int dimension;
        #endregion
        public DeterministicZcRepresentation(Currency currency, int dimension) : base(currency)
        {
            this.dimension = dimension;
        }
        public override RnRFunction Zc(DateTime date, DateTime maturity, double fwdZc)
        {
            return RnRFunctions.Constant(fwdZc, dimension);
        }
    }
    
    public class EquitySpotRepresentation
    {
        #region private fields
        private readonly DiscountCurve assetDiscountCurve;
        private readonly PaymentInfo probaMeasure;
        #endregion
        public EquitySpotRepresentation(AssetId asset, DiscountCurve assetDiscountCurve, PaymentInfo probaMeasure)
        {
            this.assetDiscountCurve = assetDiscountCurve;
            this.probaMeasure = probaMeasure;
            Asset = asset;
        }
        public RnRFunction Spot(DateTime date)
        {
            var forwardWithoutDivs = assetDiscountCurve.Zc(probaMeasure.Date) / assetDiscountCurve.Zc(date);
            return RnRFunctions.Affine(0.0, new[] { forwardWithoutDivs });
        }
        public AssetId Asset { get; private set; }
    }
    
}