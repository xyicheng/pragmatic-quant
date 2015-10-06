using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Product
{
    public abstract class Coupon : IProduct
    {
        protected Coupon(PaymentInfo paymentInfo, params IFixing[] fixings)
        {
            PaymentInfo = paymentInfo;
            Fixings = fixings;
        }

        public PaymentInfo PaymentInfo { get; private set; }
        public IFixing[] Fixings { get; private set; }
        public abstract double Payoff(double[] fixings);

        public FinancingId Financing { get { return PaymentInfo.Financing; } }
        
        public TResult Accept<TResult>(IProductVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public abstract class RateCoupon : Coupon
    {
        protected RateCoupon(FinancingId financing, Currency payCurrency, 
            double nominal, CouponSchedule schedule, DayCountFrac basis, 
            params IFixing[] fixings)
            : base(new PaymentInfo(payCurrency, schedule.Pay, financing), fixings)
        {
            Basis = basis;
            Schedule = schedule;
            PayoffAdjustement = Basis.Count(schedule.Start, schedule.End) * nominal;
        }
        protected abstract double RateFixing(double[] fixings);

        public CouponSchedule Schedule { get; private set; }
        public DayCountFrac Basis { get; private set; }
        public double PayoffAdjustement { get; set; }

        public override double Payoff(double[] fixings)
        {
            return PayoffAdjustement * RateFixing(fixings);
        }
    }

    public class FloatCoupon : RateCoupon
    {
        #region private fields
        private readonly double add;
        private readonly double mult;
        #endregion
        
        public FloatCoupon(FinancingId financing, Libor libor, double add, double mult,
            double nominal, CouponSchedule schedule, DayCountFrac basis)
            : base(financing, libor.Currency, nominal, schedule, basis, libor)
        {
            this.add = add;
            this.mult = mult;
        }

        public static FloatCoupon Standard(FinancingId financing, Libor libor, double nominal)
        {
            return new FloatCoupon(financing, libor, 0.0, 1.0, nominal, libor.Schedule, libor.Basis);
        }

        protected override double RateFixing(double[] fixings)
        {
            return add + mult * fixings[0];
        }
    }

    public class FixedCoupon : Coupon
    {
        public FixedCoupon(PaymentInfo paymentInfo, double nominal, double rateCoupon)
            : this(paymentInfo, nominal * rateCoupon) { }
        public FixedCoupon(PaymentInfo paymentInfo, double coupon)
            : base(paymentInfo)
        {
            Coupon = coupon;
        }

        public double Coupon { get; private set; }
        public override double Payoff(double[] fixings)
        {
            return Coupon;
        }
        
    }
    
    public class Leg<TCoupon>
        : IProduct where TCoupon : Coupon
    {
        public Leg(TCoupon[] coupons)
        {
            Coupons = coupons;
        }

        public TCoupon[] Coupons { get; private set; }
        public FinancingId Financing
        {
            get { return Coupons.Select(cpn => cpn.Financing).Distinct().Single(); }
        }
        public TResult Accept<TResult>(IProductVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

}