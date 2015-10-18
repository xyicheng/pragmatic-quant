using System;
using System.Diagnostics;
using System.Linq;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_model.Product.DslPayoff
{
    [DebuggerDisplay("{Expression}")]
    public class DslPayoffFunction : IFixingFunction
    {
        #region private fields
        private readonly Func<object, double[], double> payoff;
        private readonly object payoffObj;
        #endregion
        public DslPayoffFunction(IFixing[] fixings, Func<object, double[], double> payoff, object payoffObj, string expression)
        {
            Fixings = fixings;
            this.payoff = payoff;
            this.payoffObj = payoffObj;
            Expression = expression;
        }

        public string Expression { get; private set; } 

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