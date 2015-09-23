using System;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Maths.Function;

namespace pragmatic_quant_model.MarketDatas
{
    public abstract class DiscountCurve
    {
        #region private fields
        private readonly DateTime refDate;
         #endregion
        #region protected methods
        protected DiscountCurve(DateTime refDate, FinancingId financing)
        {
            this.refDate = refDate;
            Financing = financing;
        }
        #endregion
        
        public DateTime RefDate
        {
            get { return refDate; }
        }
        public FinancingId Financing { get; private set; }
        public abstract double Zc(DateTime date);
        public abstract double Zc(double d);

        public static DiscountCurve LinearRateInterpol(FinancingId financing, DateTime[] pillars, double[] zcs, ITimeMeasure time)
        {
            if (pillars.Length != zcs.Length)
                throw new Exception("LinearRateDiscountProvider : Incompatible size");
            var dates = time[pillars];

            var zcRates = new double[pillars.Length];

            if (DoubleUtils.EqualZero(dates[0]))
            {
                if (!DoubleUtils.MachineEquality(1.0, zcs[0]))
                    throw new Exception("LinearRateInterpol : Discount for refDate must equal to 1.0 ");
                zcRates[0] = 0.0;
            }
            else
            {
                zcRates[0] = -Math.Log(zcs[0]) / dates[0];
            }

            for (int i = 1; i < zcs.Length; i++)
            {
                zcRates[i] = -Math.Log(zcs[i]) / dates[i];
            }

            return new DiscountCurveFromRate(financing, time, RrFunctions.LinearInterpolation(dates, zcRates));
        }
        public static DiscountCurve Product(DiscountCurve discount1, DiscountCurve discount2, FinancingId financing)
        {
            return new ProductDiscount(discount1, discount2, financing);
        }
        public static DiscountCurve Flat(FinancingId financing, DateTime refDate, double zcRate)
        {
            var time = TimeMeasure.Act365(refDate);
            var zc1Y = Math.Exp(-zcRate * time[refDate + Duration.Year]);
            return LinearRateInterpol(financing, new[] { refDate + Duration.Year }, new[] { zc1Y }, time);
        }
    }


    public class DiscountCurveFromRate : DiscountCurve
    {
        #region private fields
        private readonly ITimeMeasure time;
        private readonly RrFunction zcRate;
        #endregion
        public DiscountCurveFromRate(FinancingId financing, ITimeMeasure time, RrFunction zcRate)
            : base(time.RefDate, financing)
        {
            this.time = time;
            this.zcRate = zcRate;
        }
        public override double Zc(DateTime date)
        {
            return Zc(time[date]);
        }
        public override double Zc(double d)
        {
            return Math.Exp(-zcRate.Eval(d) * d);
        }
    }
    
    public class ProductDiscount : DiscountCurve
    {
        #region private fields
        private readonly DiscountCurve discount1;
        private readonly DiscountCurve discount2;
        #endregion
        public ProductDiscount(DiscountCurve discount1, DiscountCurve discount2, FinancingId financing)
            : base(discount1.RefDate, financing)
        {
            if (discount1.RefDate != discount2.RefDate)
                throw new Exception("ProductDiscount : incompatible ref date !");
            
            this.discount1 = discount1;
            this.discount2 = discount2;
        }
        public override double Zc(DateTime d)
        {
            return discount1.Zc(d) * discount2.Zc(d);
        }
        public override double Zc(double d)
        {
            return discount1.Zc(d) * discount2.Zc(d);
        }
    }

}
