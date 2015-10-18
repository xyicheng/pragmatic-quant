using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_model.MonteCarlo.Product
{
    internal class CouponArrayPathFlow : IProductPathFlow
    {
        #region private fields
        private readonly CouponPathFlow[] couponPathFlows;
        #endregion
        public CouponArrayPathFlow(CouponPathFlow[] couponPathFlows)
        {
            this.couponPathFlows = couponPathFlows;
            Payments = couponPathFlows.Map(cpnCoord => cpnCoord.PaymentInfo);
            Fixings = EnumerableUtils.Merge(couponPathFlows.Map(c => c.PayoffPathValues.FixingFunction.Fixings))
                                     .OrderBy(f => f.Date)
                                     .ToArray();
        }
        
        public void ComputePathFlows(ref PathFlows<double, PaymentInfo> pathFlows, 
                                     PathFlows<double[], IFixing[]> fixingsPath, 
                                     PathFlows<double[], PaymentInfo[]> flowRebasementPath)
        {
            double[][] fixings = fixingsPath.Flows;
            double[][] rebasements = flowRebasementPath.Flows;

            double[] couponsFlows = pathFlows.Flows;
            for (int i = 0; i < couponPathFlows.Length; i++)
            {
                couponsFlows[i] = couponPathFlows[i].FlowValue(fixings, rebasements);
            }
        }
        
        public PathFlows<double, PaymentInfo> NewPathFlow()
        {
            return new PathFlows<double, PaymentInfo>(new double[couponPathFlows.Length], Payments);
        }
        public int SizeOfPath
        {
            get { return couponPathFlows.Length * sizeof(double); }
        }

        public IFixing[] Fixings { get; private set; }
        public PaymentInfo[] Payments { get; private set; }
    }
    
}