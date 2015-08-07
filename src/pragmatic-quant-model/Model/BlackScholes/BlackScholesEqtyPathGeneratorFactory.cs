using System;
using System.Diagnostics.Contracts;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths.Stochastic;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.Model.BlackScholes
{

    public class BlackScholesEqtyPathGeneratorFactory : ModelPathGenereratorFactory<BlackScholesModel>
    {
        #region private methods
        private BsEqtySimulatorStepDatas StepSchedule(DateTime start, DateTime end, BlackScholesModel model, DiscountCurve assetDiscount, DateTime horizon)
        {
            DiscreteLocalDividend[] stepDividends = model.Dividends
                .Where(div => start < div.Date && div.Date <= end)
                .OrderBy(div => div.Date)
                .ToArray();
            //If step end date is not a div date, we insert a fictious zero dividend
            if (!(stepDividends.Any() && end.Equals(stepDividends.Last().Date)))
                stepDividends = stepDividends.Union(new[] {DiscreteLocalDividend.ZeroDiv(end)}).ToArray();
            
            var variance = (model.Sigma * model.Sigma).Integral(0.0);
            var stepVols = new double[stepDividends.Length];
            var stepVarDrifts = new double[stepDividends.Length];
            for (int i = 0; i < stepDividends.Length; i++)
            {
                var subStepStart = i > 0 ? stepDividends[i - 1].Date : start;
                var subStepEnd = stepDividends[i].Date;
                
                var subStep = RealInterval.Compact(model.Time[subStepStart], model.Time[subStepEnd]);
                var stepVariance = variance.Eval(subStep.Sup) - variance.Eval(subStep.Inf);
                var stepVarDrift = -0.5 * stepVariance;

                stepVols[i] = Math.Sqrt(stepVariance / subStep.Length);
                stepVarDrifts[i] = stepVarDrift;
            }

            var horizonDiscount = assetDiscount.Zc(horizon);
            var discounts = stepDividends.Map(div => horizonDiscount / assetDiscount.Zc(div.Date));

            var dates = model.Time[stepDividends.Map(div => div.Date)];
            var step = RealInterval.Compact(model.Time[start], model.Time[end]);
            return new BsEqtySimulatorStepDatas(step, dates, stepDividends, discounts, stepVols, stepVarDrifts);
        }
        #endregion
        public static readonly BlackScholesEqtyPathGeneratorFactory Instance = new BlackScholesEqtyPathGeneratorFactory();
        protected override IProcessPathGenerator Build(BlackScholesModel model, Market market, PaymentInfo probaMeasure, DateTime[] simulatedDates)
        {
            var asset = model.Asset;
            if (!probaMeasure.Financing.Currency.Equals(probaMeasure.Currency)
                || !probaMeasure.Currency.Equals(asset.Currency))
                throw new NotImplementedException("TODO !");

            var numeraireDiscount = market.DiscountCurve(probaMeasure.Financing);
            var assetMkt = market.AssetMarket(asset);
            DiscountCurve assetDiscount = assetMkt.AssetFinancingCurve(numeraireDiscount);
            var forward = assetMkt.Spot / assetDiscount.Zc(probaMeasure.Date);
            
            BsEqtySimulatorStepDatas[] stepSimulDatas = EnumerableUtils.For(0, simulatedDates.Length,
                i => StepSchedule(i > 0 ? simulatedDates[i - 1] : market.RefDate, simulatedDates[i], 
                                  model, assetDiscount, probaMeasure.Date));
            
            return new BlackScholesEquityPathGenerator(stepSimulDatas, forward);
        }
    }
    
    public class BsEqtySimulatorStepDatas
    {
        public BsEqtySimulatorStepDatas(RealInterval step, double[] dates, DiscreteLocalDividend[] dividends, double[] discounts,
            double[] vols, double[] varDrifts)
        {
            Contract.Requires(dividends.Length == vols.Length);
            Contract.Requires(dates.All(step.Contain));
            Contract.Requires(EnumerableUtils.IsSorted(dividends.Map(div => div.Date)));

            Step = step;
            Dates = dates;
            Dividends = dividends;
            Discounts = discounts;
            Vols = vols;
            VarDrifts = varDrifts;
        }
        public RealInterval Step { get; private set; }
        public double[] Dates { get; private set; }
        public DiscreteLocalDividend[] Dividends { get; private set; }
        public double[] Vols { get; private set; }
        public double[] VarDrifts { get; private set; }
        public double[] Discounts { get; private set; }
    }
    
    public class BlackScholesEquityPathGenerator : IProcessPathGenerator
    {
        #region private fields
        private readonly double[] pathDates;
        private readonly BrownianBridge brownianBridge;
        private readonly double[][] vols;
        private readonly double[][] varDrifts;
        private readonly double[][] discounts;
        private readonly DiscreteLocalDividend[][] divs;
        private readonly double initialFwd;
        #endregion
        public BlackScholesEquityPathGenerator(BsEqtySimulatorStepDatas[] stepDatas, double forward)
        {
            var allDates = EnumerableUtils.Append(stepDatas.Map(step => step.Dates));
            brownianBridge = BrownianBridge.Create(allDates);
            initialFwd = forward;

            pathDates = stepDatas.Map(step => step.Step.Sup);
            vols = stepDatas.Map(step => step.Vols);
            varDrifts = stepDatas.Map(step => step.VarDrifts);
            discounts = stepDatas.Map(step => step.Discounts);
            divs = stepDatas.Map(step => step.Dividends);

            RandomDim = brownianBridge.GaussianSize(1);
        }

        public IProcessPath Path(double[] gaussians)
        {
            var dWs = brownianBridge.PathIncrements(gaussians, 1);
            
            var processPath = new double[pathDates.Length][];
            double currentFwd = initialFwd;
            int brownianIndex = 0;
            for (int step = 0; step < pathDates.Length; step++)
            {
                var stepVols = vols[step];
                var stepVarDrifts = varDrifts[step];
                var stepDivs = divs[step];
                var stepDiscount = discounts[step];
                for (int subStep = 0; subStep < stepVols.Length; subStep++)
                {
                    var dW = dWs[brownianIndex++][0];
                    double fwdBeforeDiv = currentFwd * Math.Exp(stepVols[subStep] * dW + stepVarDrifts[subStep]);
                    double discount = stepDiscount[subStep];
                    double div = stepDivs[subStep].Value(fwdBeforeDiv * discount);
                    currentFwd = Math.Max(0.0, fwdBeforeDiv - div / discount);
                }
                processPath[step] = new[] {currentFwd};
            }

            return new ProcessPath(pathDates, 1, processPath);
        }
        public double[] Dates { get { return pathDates; } }
        public int ProcessDim { get { return 1; } }
        public int RandomDim { get; private set; }
    }

}