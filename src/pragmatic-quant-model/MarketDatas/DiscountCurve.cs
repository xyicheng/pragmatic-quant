using System;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.MarketDatas
{
    public abstract class DiscountCurve
    {
        #region protected fields
        protected readonly DateTime refDate;
        #endregion
        #region protected methods
        protected DiscountCurve(DateTime refDate)
        {
            this.refDate = refDate;
        }
        #endregion
        public DateTime RefDate
        {
            get { return refDate; }
        }
        public abstract double Zc(DateTime d);

        public static DiscountCurve LinearRateInterpol(DateTime[] pillars, double[] zcs, ITimeMeasure time)
        {
            return new LinearZcRateInterpolation(pillars, zcs, time);
        }
        public static DiscountCurve Product(DiscountCurve discount1, DiscountCurve discount2)
        {
            return new ProductDiscount(discount1, discount2);
        }
    }

    public class LinearZcRateInterpolation : DiscountCurve
    {
        #region private fields
        private readonly ITimeMeasure time;
        private readonly StepSearcher stepSearcher;
        private readonly double[] dates;
        private readonly double[] zcRates;
        #endregion
        public LinearZcRateInterpolation(DateTime[] pillars, double[] zcs, ITimeMeasure time)
            : base(time.RefDate)
        {
            if (pillars.Length != zcs.Length)
                throw new Exception("LinearRateDiscountProvider : Incompatible size");
            this.time = time;
            dates = time[pillars];
            stepSearcher = new StepSearcher(dates);
            zcRates = new double[pillars.Length];

            if (pillars[0] == refDate)
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
            var leftIndex = stepSearcher.LocateLeftIndex(t);

            if (leftIndex <= -1)
            {
                return Math.Exp(-zcRates[0] * t);
            }

            if (leftIndex >= dates.Length - 1)
            {
                return Math.Exp(-zcRates[zcRates.Length - 1] * t);
            }

            var w = (t - dates[leftIndex]) / (dates[leftIndex + 1] - dates[leftIndex]);
            var rate = (1.0 - w) * zcRates[leftIndex] + w * zcRates[leftIndex + 1];
            return Math.Exp(-rate * t);
        }
    }

    public class ProductDiscount : DiscountCurve
    {
        #region private fields
        private readonly DiscountCurve discount1;
        private readonly DiscountCurve discount2;
        #endregion
        public ProductDiscount(DiscountCurve discount1, DiscountCurve discount2)
            : base(discount1.RefDate)
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
