using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Model.HullWhite
{
    public class Hw1ModelDescription : IModelDescription
    {
        public Hw1ModelDescription(Currency currency, double meanReversion, MapRawDatas<DateOrDuration, double> sigma)
        {
            Sigma = sigma;
            MeanReversion = meanReversion;
            Currency = currency;
        }
        public Currency Currency { get; private set; }
        public double MeanReversion { get; private set; }
        public MapRawDatas<DateOrDuration, double> Sigma { get; private set; }
    }

    internal class Hw1ModelFactory : ModelFactory<Hw1ModelDescription>
    {
        public static readonly Hw1ModelFactory Instance = new Hw1ModelFactory();
        public override IModel Build(Hw1ModelDescription model, Market market)
        {
            var time = ModelFactoryUtils.DefaultTime(market.RefDate);
            return new Hw1Model(time, model.Currency, model.MeanReversion, model.Sigma.ToFunction(time));
        }
    }

}