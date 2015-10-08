using System;
using System.Linq;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_model.Product.CouponDsl
{
    public class DslPayoffFunction : IFixingFunction
    {
        #region private fields
        private readonly Func<object, double[], double> payoff;
        private readonly object payoffObj;
        #endregion
        public DslPayoffFunction(IFixing[] fixings, Func<object, double[], double> payoff, object payoffObj)
        {
            Fixings = fixings;
            this.payoff = payoff;
            this.payoffObj = payoffObj;
        }

        public DateTime Date
        {
            get
            {
                return Fixings.Length > 0
                       ? Fixings.Select(f => f.Date).Max()
                       : DateTime.MinValue;
            }
        }
        public IFixing[] Fixings { get; private set; }
        public double Value(double[] fixings)
        {
            return payoff(payoffObj, fixings);
        }
    }
}