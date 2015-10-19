using System.Diagnostics.Contracts;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_model.MonteCarlo.Product
{

    internal class AutocallPathFlow : IProductPathFlow
    {
        #region private fields
        private readonly CouponPathFlow[] underlyingPathFlows;
        private readonly CouponPathFlow[] redemptionPathFlows;
        private readonly FixingFuncPathValue[] triggers;
        private readonly int[] underlyingCallIndexes;
        private readonly CouponFlowLabel[] labels;
        #endregion
        #region private methods
        private int CallIndex(double[][] fixings)
        {
            int triggerIndex = 0;
            for (; triggerIndex < triggers.Length; triggerIndex++)
            {
                var trigger = triggers[triggerIndex].Value(fixings);

                if (trigger > 0.5)
                    break;
            }
            return triggerIndex;
        }
        #endregion
        public AutocallPathFlow(CouponPathFlow[] underlyingPathFlows,
                                CouponPathFlow[] redemptionPathFlows,
                                FixingFuncPathValue[] triggers,
                                int[] underlyingCallIndexes)
        {
            Contract.Requires(redemptionPathFlows.Length == triggers.Length);
            Contract.Requires(underlyingCallIndexes.Length == triggers.Length);

            this.underlyingPathFlows = underlyingPathFlows;
            this.redemptionPathFlows = redemptionPathFlows;
            this.triggers = triggers;
            this.underlyingCallIndexes = underlyingCallIndexes;

            var underlyingFixings = underlyingPathFlows.Map(pf => pf.PayoffPathValues.FixingFunction.Fixings);
            var redemptionFixings = redemptionPathFlows.Map(pf => pf.PayoffPathValues.FixingFunction.Fixings);
            var triggerFixings = triggers.Map(pf => pf.FixingFunction.Fixings);
            Fixings = EnumerableUtils.Merge(underlyingFixings)
                .MergeWith(redemptionFixings)
                .MergeWith(triggerFixings);

            var underlyingLabels = underlyingPathFlows.Map(pf => pf.CouponLabel);
            var redemptionLabels = redemptionPathFlows.Map(pf => pf.CouponLabel);
            labels = EnumerableUtils.Append(underlyingLabels, redemptionLabels);

            var underlyingPayments = underlyingLabels.Map(l => l.Payment);
            var redemptionPayments = redemptionLabels.Map(l => l.Payment);
            Payments = EnumerableUtils.Append(underlyingPayments, redemptionPayments);
        }

        public void ComputePathFlows(ref PathFlows<double, CouponFlowLabel> pathFlows,
                                     PathFlows<double[], IFixing[]> fixingsPath,
                                     PathFlows<double[], PaymentInfo[]> flowRebasementPath)
        {
            double[][] fixings = fixingsPath.Flows;
            double[][] rebasements = flowRebasementPath.Flows;

            double[] autocallFlows = pathFlows.Flows;
            for (int i = 0; i < autocallFlows.Length; i++)
                autocallFlows[i] = 0.0;

            int callIndex = CallIndex(fixings);
            int lastPayCouponIndex = (callIndex < underlyingCallIndexes.Length)
                ? underlyingCallIndexes[callIndex]
                : underlyingPathFlows.Length;

            for (int i = 0; i < lastPayCouponIndex; i++)
                autocallFlows[i] = underlyingPathFlows[i].FlowValue(fixings, rebasements);

            if (callIndex < redemptionPathFlows.Length)
            {
                autocallFlows[underlyingPathFlows.Length + callIndex] = redemptionPathFlows[callIndex].FlowValue(fixings, rebasements);
            }
        }
        public PathFlows<double, CouponFlowLabel> NewPathFlow()
        {
            return new PathFlows<double, CouponFlowLabel>(new double[Payments.Length], labels);
        }
        public int SizeOfPath
        {
            get { return Payments.Length * sizeof (double); }
        }
        public IFixing[] Fixings { get; private set; }
        public PaymentInfo[] Payments { get; private set; }
    }

}