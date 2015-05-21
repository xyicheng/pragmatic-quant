using System;
using System.Diagnostics.Contracts;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;

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

    public class Hw1FactorRepresentationFactory : IFactorRepresentationFactory<Hw1Model>
    {
        public IFactorModelRepresentation Build(Hw1Model model, Market market)
        {
            var zcRepresentation = new Hw1ModelZcRepresentation(model);
            return new FactorRepresentation(market, zcRepresentation);
        }
    }
}