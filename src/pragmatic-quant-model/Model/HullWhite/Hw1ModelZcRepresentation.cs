using System;
using System.Diagnostics.Contracts;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.Model.HullWhite
{
    public class Hw1ModelZcRepresentation : RateZcRepresentation
    {
        #region private fields
        private readonly ITimeMeasure time;
        private readonly double meanReversion;
        private readonly RrFunction driftTerm;
        #endregion
        public Hw1ModelZcRepresentation(Hw1Model hw1Model)
            : base(hw1Model.Currency)
        {
            time = hw1Model.Time;
            meanReversion = hw1Model.MeanReversion;
            driftTerm = hw1Model.DriftTerm();
        }
        public override RnRFunction Zc(DateTime date, DateTime maturity, double fwdZc)
        {
            Contract.Requires(date <= maturity);
            double d = time[date];
            var drift = driftTerm.Eval(d);
            return HwModelUtils.ZcFunction(time[maturity] - d, fwdZc, new[] {meanReversion}, new[,] {{drift}});
        }
    }

    public class Hw1FactorRepresentationFactory : FactorRepresentationFactory<Hw1Model>
    {
        public static readonly FactorRepresentationFactory<Hw1Model> Instance = new Hw1FactorRepresentationFactory();
        protected override IFactorModelRepresentation Build(Hw1Model model, Market market, PaymentInfo probaMeasure)
        {
            var zcRepresentation = new Hw1ModelZcRepresentation(model);
            return new FactorRepresentation(market, zcRepresentation);
        }
    }
}