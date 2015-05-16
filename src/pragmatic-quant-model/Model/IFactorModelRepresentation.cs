using System;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.Model
{
    public interface IFactorModelRepresentation
    {
        Func<double[], double> this[IObservation observation] { get; }
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

    public abstract class EquitySpotRepresentation
    {
        protected EquitySpotRepresentation(AssetId asset)
        {
            Asset = asset;
        }
        public abstract RnRFunction Spot(DateTime date, double initialSpot);
        public AssetId Asset { get; private set; }
    }

    public class DeterministicZcRepresentation : RateZcRepresentation
    {
        public DeterministicZcRepresentation(Currency currency) : base(currency)
        {
        }
        public override RnRFunction Zc(DateTime date, DateTime maturity, double fwdZc)
        {
            return RnRFunctions.Constant(fwdZc, 0);
        }
    }
}