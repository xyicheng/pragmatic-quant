using System;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Interpolation;

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
        public abstract double Zc(DateTime d);

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

            return new DiscountCurveFromRate(financing, time, new LinearInterpolation(dates, zcRates, 0.0, 0.0));
        }
        public static DiscountCurve Product(DiscountCurve discount1, DiscountCurve discount2, FinancingId financing)
        {
            return new ProductDiscount(discount1, discount2, financing);
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
        public override double Zc(DateTime d)
        {
            double t = time[d];
            return Math.Exp(-zcRate.Eval(t) * t);
        }
    }
    
    /*
    public class LinearZcRateInterpolation : DiscountCurve
    {
        #region private fields
        private readonly ITimeMeasure time;
        private readonly StepSearcher stepSearcher;
        private readonly double[] dates;
        private readonly double[] zcRates;
        #endregion
        #region private fields
        private double ZcRate(double t)
        {
            var leftIndex = stepSearcher.LocateLeftIndex(t);

            if (leftIndex <= -1)
            {
                return zcRates[0];
            }

            if (leftIndex >= dates.Length - 1)
            {
                return zcRates[zcRates.Length - 1];
            }

            var w = (t - dates[leftIndex]) / (dates[leftIndex + 1] - dates[leftIndex]);
            return (1.0 - w) * zcRates[leftIndex] + w * zcRates[leftIndex + 1];
        }
        #endregion
        public LinearZcRateInterpolation(FinancingId financing, DateTime[] pillars, double[] zcs, ITimeMeasure time)
            : base(time.RefDate, financing)
        {
            if (pillars.Length != zcs.Length)
                throw new Exception("LinearRateDiscountProvider : Incompatible size");
            this.time = time;
            dates = time[pillars];
            stepSearcher = new StepSearcher(dates);
            zcRates = new double[pillars.Length];

            if (pillars[0] == RefDate)
            {
                if (!DoubleUtils.MachineEquality(1.0, zcs[0]))
                    throw new Exception("LinearRateDiscountProvider : Discount for refDate must equal to 1.0 ");
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
        }

        public override double Zc(DateTime d)
        {
            double t = time[d];
            return Math.Exp(-ZcRate(t) * t);
        }
    }
    */

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
    }
}
