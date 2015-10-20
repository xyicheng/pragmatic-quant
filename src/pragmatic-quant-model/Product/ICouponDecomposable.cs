using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_model.Product
{
    public interface ICouponDecomposable : IProduct
    {
        Coupon[] Decomposition();
    }

    public class Leg<TCoupon> : ICouponDecomposable 
        where TCoupon : Coupon
    {
        #region private fields
        private readonly TCoupon[] coupons;
        private readonly FinancingId financing;
        #endregion
        public Leg(TCoupon[] coupons)
        {
            this.coupons = coupons;
            financing = coupons.Select(cpn => cpn.Financing).Distinct().Single();
        }
        
        public FinancingId Financing
        {
            get
            {
                return financing;
            }
        }
        public TResult Accept<TResult>(IProductVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
        public Coupon[] Decomposition()
        {
            return coupons.Cast<Coupon>().ToArray();
        }
    }

    public class DecomposableLinearCombination : ICouponDecomposable
    {
        #region private fields
        private readonly double[] weights; 
        private readonly ICouponDecomposable[] decomposables;
        #endregion
        #region private methods
        private DecomposableLinearCombination(double[] weights, ICouponDecomposable[] decomposables)
        {
            Debug.Assert(weights.Length == decomposables.Length);
            Financing = decomposables.Select(d => d.Financing).Distinct().Single();
            this.weights = weights;
            this.decomposables = decomposables;
        }
        #endregion

        public static DecomposableLinearCombination Create(double[] weights, ICouponDecomposable[] decomposables)
        {
            Debug.Assert(weights.Length == decomposables.Length);
            var weightsList = new List<double>();
            var decompList = new List<ICouponDecomposable>();
            for (int i = 0; i < weights.Length; i++)
            {
                double weight = weights[i];
                ICouponDecomposable decomp = decomposables[i];
                var lc = decomp as DecomposableLinearCombination;
                if (lc != null)
                {
                    decompList.AddRange(lc.decomposables);
                    weightsList.AddRange(lc.weights.Map(w => w * weight));
                }
                else
                {
                    decompList.Add(decomp);
                    weightsList.Add(weight);
                }
            }
            return new DecomposableLinearCombination(weightsList.ToArray(), decompList.ToArray());
        }

        public FinancingId Financing { get; private set; }
        public TResult Accept<TResult>(IProductVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
        public Coupon[] Decomposition()
        {
            Coupon[][] weightedCoupons = weights.ZipWith(decomposables, (w, d) =>
            {
                var coupons = d.Decomposition();
                return coupons.Map(cpn => new Coupon(cpn.PaymentInfo, new WeightedFixingFunction(w, cpn.Payoff)));
            });
            return EnumerableUtils.Append(weightedCoupons);
        }
    }

}