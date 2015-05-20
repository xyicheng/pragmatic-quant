using System.Linq;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Product
{
    public abstract class Coupon : IProduct
    {
        protected Coupon(PaymentInfo paymentInfo, double nominal)
        {
            PaymentInfo = paymentInfo;
            Nominal = nominal;
        }

        public abstract IFixing[] Fixings { get; }
        public abstract double Payoff(double[] fixings);
        public double Nominal { get; private set; }
        public PaymentInfo PaymentInfo { get; private set; }

        public FinancingId Financing { get { return PaymentInfo.Financing; } }
        public abstract TResult Accept<TResult>(IProductVisitor<TResult> visitor);
    }

    public abstract class RateCoupon : Coupon
    {
        protected RateCoupon(FinancingId financing, Currency payCurrency, double nominal, CouponSchedule schedule, DayCountFrac basis)
            : base(new PaymentInfo(payCurrency, schedule.Pay, financing), nominal)
        {
            Basis = basis;
            Schedule = schedule;
            PayoffAdjustement = Basis.Count(schedule.Start, schedule.End) * nominal;
        }
        public CouponSchedule Schedule { get; private set; }
        public DayCountFrac Basis { get; private set; }
        public double PayoffAdjustement { get; set; }
    }

    public class FloatCoupon : RateCoupon
    {
        #region private fields
        private readonly Libor libor;
        private readonly double add;
        private readonly double mult;
        #endregion
        
        public FloatCoupon(FinancingId financing, Libor libor, double add, double mult,
            double nominal, CouponSchedule schedule, DayCountFrac basis)
            : base(financing, libor.Currency, nominal, schedule, basis)
        {
            this.libor = libor;
            this.add = add;
            this.mult = mult;
        }

        public static FloatCoupon Standard(FinancingId financing, Libor libor, double nominal)
        {
            return new FloatCoupon(financing, libor, 0.0, 1.0, nominal, libor.Schedule, libor.Basis);
        }

        public override IFixing[] Fixings
        {
            get { return new IFixing[] {libor}; }
        }
        public override double Payoff(double[] fixings)
        {
            return add + mult * fixings[0];
        }
        public override TResult Accept<TResult>(IProductVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
        }
    }

    public class FixedCoupon : Coupon
    {
        public FixedCoupon(PaymentInfo paymentInfo, double nominal, double coupon)
            : base(paymentInfo, nominal)
        {
            Coupon = coupon;
        }

        public double Coupon { get; private set; }
        public override IFixing[] Fixings
        {
            get { return new IFixing[0]; }
        }
        public override double Payoff(double[] fixings)
        {
            return Coupon;
        }
        public override TResult Accept<TResult>(IProductVisitor<TResult> visitor)
        {
            return visitor.Visit(this);
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