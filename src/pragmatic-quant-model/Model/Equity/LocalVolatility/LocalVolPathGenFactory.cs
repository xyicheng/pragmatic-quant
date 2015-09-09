using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Stochastic;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.Model.Equity.LocalVolatility
{
    public class LocalVolPathGenFactory : ModelPathGenereratorFactory<LocalVolatilityModel>
    {
        #region private fields
        private readonly MonteCarloConfig mcConfig;
        #endregion
        #region private methods
        private static Func<double, double> LocalVol(DateTime date, LocalVolatilityModel model)
        {
            MoneynessProvider moneyness = model.Moneyness;
            double t = model.Time[date];
            RrFunction localVariance = model.LocalVariance.TimeSlice(t);

            //TODO very slow !!!
            return logSpot => Math.Sqrt(localVariance.Eval(moneyness.Moneyness(t, Math.Exp(logSpot)))); 
        }
        private void StepSchedule(DateTime start, DateTime end, EquityModel model,
            out DateTime[] dates, out bool[] isDivDate, out DiscreteLocalDividend[] dividends)
        {
            DiscreteLocalDividend[] stepDividends = model.Dividends.Where(div => start < div.Date && div.Date <= end)
                .OrderBy(div => div.Date)
                .ToArray();

            var datesList = new List<DateTime>();
            var isDivDateList = new List<bool>();
            var dividendsList = new List<DiscreteLocalDividend>();

            var previous = start;
            foreach (DiscreteLocalDividend div in stepDividends)
            {
                DateTime[] stepSchedule = ScheduleUtils.RawSchedule(previous, div.Date, mcConfig.McStep, true);
                datesList.AddRange(stepSchedule);

                if (stepSchedule.Length > 1)
                {
                    dividendsList.AddRange(stepSchedule.Take(stepSchedule.Length - 1).Map(DiscreteLocalDividend.ZeroDiv));
                    isDivDateList.AddRange(Enumerable.Repeat(false, stepSchedule.Length - 1));
                }
                dividendsList.Add(div);
                isDivDateList.Add(true);

                previous = div.Date;
            }

            if (previous < end)
            {
                DateTime[] stepSchedule = ScheduleUtils.RawSchedule(previous, end, mcConfig.McStep, true);
                datesList.AddRange(stepSchedule);
                dividendsList.AddRange(stepSchedule.Take(stepSchedule.Length).Map(DiscreteLocalDividend.ZeroDiv));
                isDivDateList.AddRange(Enumerable.Repeat(false, stepSchedule.Length));
            }

            dates = datesList.ToArray();
            isDivDate = isDivDateList.ToArray();
            dividends = dividendsList.ToArray();
        }
        private LocalVolSimulatorStepDatas StepSimulDatas(DateTime start, DateTime end, LocalVolatilityModel model, DiscountCurve assetDiscount, DateTime horizon)
        {
            DateTime[] dates;
            bool[] isDivDate;
            DiscreteLocalDividend[] dividends;
            StepSchedule(start, end, model, out dates, out isDivDate, out dividends);
            
            var horizonDiscount = assetDiscount.Zc(horizon);
            var discounts = dividends.Map(div => horizonDiscount / assetDiscount.Zc(div.Date));
            var localVols = dates.Map(d => LocalVol(d, model));
            var step = RealInterval.Compact(model.Time[start], model.Time[end]);
            return new LocalVolSimulatorStepDatas(step, dates.Map(d => model.Time[d]), isDivDate, dividends, discounts, localVols);
        }
        #endregion
        public LocalVolPathGenFactory(MonteCarloConfig mcConfig)
        {
            this.mcConfig = mcConfig;
            if (mcConfig.McStep == null)
                throw new ArgumentException("LocalVol model resuires McStep");
        }
        protected override IProcessPathGenerator Build(LocalVolatilityModel model, Market market, PaymentInfo probaMeasure, DateTime[] simulatedDates)
        {
            var asset = model.Asset;
            var assetMkt = market.AssetMarket(asset);

            if (!probaMeasure.Financing.Currency.Equals(probaMeasure.Currency)
                || !probaMeasure.Currency.Equals(asset.Currency))
                throw new NotImplementedException("TODO !");

            var numeraireDiscount = market.DiscountCurve(probaMeasure.Financing);
            DiscountCurve assetDiscount = assetMkt.AssetFinancingCurve(numeraireDiscount);
            double forward = assetMkt.Spot / assetDiscount.Zc(probaMeasure.Date);

            LocalVolSimulatorStepDatas[] stepSimulDatas = EnumerableUtils.For(0, simulatedDates.Length,
                i => StepSimulDatas(i > 0 ? simulatedDates[i - 1] : market.RefDate, simulatedDates[i],
                                  model, assetDiscount, probaMeasure.Date));

            return new LocalVolEquityPathGenerator(stepSimulDatas, forward);
        }
    }

    public class LocalVolSimulatorStepDatas
    {
        public LocalVolSimulatorStepDatas(RealInterval step, double[] dates, 
            bool[] isDivDates, DiscreteLocalDividend[] dividends,
            double[] discounts, Func<double, double>[] localVols)
        {
            Contract.Requires(dates.Length == isDivDates.Length);
            Contract.Requires(isDivDates.Length == dividends.Length);
            Contract.Requires(dividends.Length == discounts.Length);
            Contract.Requires(discounts.Length == localVols.Length);
            Contract.Requires(dates.All(step.Contain));
            Contract.Requires(EnumerableUtils.IsSorted(dividends.Map(div => div.Date)));

            Step = step;
            Dates = dates;
            IsDivDates = isDivDates;
            Dividends = dividends;
            Discounts = discounts;
            LocalVols = localVols;
        }
        public RealInterval Step { get; private set; }
        
        public double[] Dates { get; private set; }
        public Func<double, double>[] LocalVols { get; private set; }
        public double[] Discounts { get; private set; }
        public bool[] IsDivDates { get; private set; }
        public DiscreteLocalDividend[] Dividends { get; private set; }
    }

    public class LocalVolEquityPathGenerator : IProcessPathGenerator
    {
        #region private fields
        private readonly Func<double, double>[][] localVols;
        private readonly double[][] dts;
        private readonly bool[][] isDivDates;
        private readonly double[][] discounts;
        private readonly DiscreteLocalDividend[][] divs;
        private readonly double initialFwd;
        #endregion
        public LocalVolEquityPathGenerator(LocalVolSimulatorStepDatas[] stepDatas, double forward)
        {
            initialFwd = forward;
            localVols = stepDatas.Map(step => step.LocalVols);
            discounts = stepDatas.Map(step => step.Discounts);
            divs = stepDatas.Map(step => step.Dividends);
            isDivDates = stepDatas.Map(step => step.IsDivDates);

            dts = stepDatas.Map(step =>
            {
                var stepDates = step.Dates;
                var stepDts = new double[stepDates.Length];
                double prev = step.Step.Inf;
                for (int i = 0; i < stepDts.Length; i++)
                {
                    stepDts[i] = stepDates[i] - prev;
                    prev = stepDates[i];
                }
                return stepDts;
            });

            Dates = stepDatas.Map(step => step.Step.Sup);
            AllSimulatedDates = EnumerableUtils.Append(stepDatas.Map(data => data.Dates));
        }

        public IProcessPath Path(double[][] dWs)
        {
            var processPath = new double[Dates.Length][];
            double currentLogFwd = Math.Log(initialFwd);
            int brownianIndex = 0;
            for (int step = 0; step < Dates.Length; step++)
            {
                var stepLocVols = localVols[step];
                var stepDts = dts[step];
                var stepIsDiv = isDivDates[step];
                var stepDivs = divs[step];
                var stepDiscount = discounts[step];

                for (int subStep = 0; subStep < stepLocVols.Length; subStep++)
                {
                    double dW = dWs[brownianIndex++][0];
                    double discount = stepDiscount[subStep];
                    double localVol = stepLocVols[subStep](currentLogFwd + Math.Log(discount)); //TODO precompute
                    double dt = stepDts[subStep];
                    currentLogFwd += localVol * dW - 0.5 * dt * localVol * localVol;

                    if (stepIsDiv[subStep])
                    {
                        double spotBeforeDiv = Math.Exp(currentLogFwd) * discount;
                        double div = stepDivs[subStep].Value(spotBeforeDiv);
                        currentLogFwd = Math.Log(Math.Max(0.0, (spotBeforeDiv - div) / discount));
                    }
                }
                processPath[step] = new[] { Math.Exp(currentLogFwd) };
            }

            return new ProcessPath(Dates, 1, processPath);
        }
        public double[] Dates { get; private set; }
        public int ProcessDim { get { return 1; } }
        public double[] AllSimulatedDates { get; private set; }
    }
}