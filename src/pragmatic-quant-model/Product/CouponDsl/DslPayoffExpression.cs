using System.Collections.Generic;

namespace pragmatic_quant_model.Product.CouponDsl
{
    public class DslPayoffExpression
    {
        public DslPayoffExpression(IFixing[] fixings, string expression, string fixingArrayId)
        {
            Fixings = fixings;
            Expression = expression;
            FixingArrayId = fixingArrayId;
        }
        
        /// <summary>
        /// C# code that represent coupon payoff
        /// </summary>
        public string Expression { get; private set; }

        /// <summary>
        /// Identifier of fixing array used in Expression
        /// </summary>
        public string FixingArrayId { get; private set; }
        
        /// <summary>
        /// Fixings of the payoff
        /// </summary>
        public IReadOnlyCollection<IFixing> Fixings { get; private set; }
    }
}