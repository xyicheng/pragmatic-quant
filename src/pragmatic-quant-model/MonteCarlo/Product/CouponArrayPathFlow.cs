using System;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Product;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_model.MonteCarlo.Product
{
    internal class CouponArrayPathFlow : IProductPathFlow
    {
        #region private fields
        private readonly Tuple<int, int>[][] fixingsIndexes;
        private readonly Tuple<int, int>[] flowRebasementIndex;
        private readonly IFixingFunction[] couponPayoffs;
        #endregion
        #region private fields (buffer)
        private readonly double[][] cpnFixingByDates;
        #endregion
        #region private methods
        private double[][] CouponFixingBuffer()
        {
            var buffer = new double[couponPayoffs.Length][];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = new double[fixingsIndexes[i].Length];
            }
            return buffer;
        }
        #endregion
        public CouponArrayPathFlow(Tuple<int, int>[][] fixingsIndexes, Tuple<int, int>[] flowRebasementIndex, Coupon[] coupons)
        {
            this.fixingsIndexes = fixingsIndexes;
            this.flowRebasementIndex = flowRebasementIndex;
            
            Payments = coupons.Map(cpn => cpn.PaymentInfo);
            Fixings = coupons.Aggregate(new IFixing[0], (prev, cpn) => prev.Union(cpn.Fixings).ToArray())
                .OrderBy(f => f.Date)
                .ToArray();
            couponPayoffs = coupons.Map(cpn => cpn.Payoff);

            //Buffer initialization
            cpnFixingByDates = CouponFixingBuffer();
        }
        
        public void ComputePathFlows(ref PathFlows<double, PaymentInfo> pathFlows, 
            PathFlows<double[], IFixing[]> fixingsPath, 
            PathFlows<double[], PaymentInfo[]> flowRebasementPath)
        {
            double[][] fixings = fixingsPath.Flows;
            double[][] rebasements = flowRebasementPath.Flows;

            double[] couponsFlows = pathFlows.Flows;
            for (int i = 0; i < couponPayoffs.Length; i++)
            {
                Tuple<int, int>[] cpnCoordinates = fixingsIndexes[i];
                double[] couponFixings = cpnFixingByDates[i];
                for (int j = 0; j < cpnCoordinates.Length; j++)
                {
                    Tuple<int, int> coordinate = cpnCoordinates[j];
                    couponFixings[j] = fixings[coordinate.Item1][coordinate.Item2];
                }

                double flowRebasement = rebasements[flowRebasementIndex[i].Item1][flowRebasementIndex[i].Item2];
                couponsFlows[i] = couponPayoffs[i].Value(couponFixings) * flowRebasement;
            }
        }
        
        public PathFlows<double, PaymentInfo> NewPathFlow()
        {
            return new PathFlows<double, PaymentInfo>(new double[couponPayoffs.Length], Payments);
        }
        public int SizeOfPath
        {
            get { return couponPayoffs.Length * sizeof (double); }
        }

        public IFixing[] Fixings { get; private set; }
        public PaymentInfo[] Payments { get; private set; }

    }
}