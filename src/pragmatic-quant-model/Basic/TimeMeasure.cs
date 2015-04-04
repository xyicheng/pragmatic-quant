using System;

namespace pragmatic_quant_model.Basic
{
    public interface ITimeMeasure
    {
        DateTime RefDate { get; }
        double this[DateTime date] { get; }
        double[] this[DateTime[] dates] { get; }
    }

    public class TimeMeasure : ITimeMeasure
    {
        #region private fields
        private readonly double yearBasis;
        #endregion
        #region private methods
        private TimeMeasure(DateTime refDate, double yearBasis)
        {
            RefDate = refDate;
            this.yearBasis = yearBasis;
        }
        #endregion
        public static ITimeMeasure Act365(DateTime refDate)
        {
            return new TimeMeasure(refDate, 365.0);
        }
        public static ITimeMeasure Act360(DateTime refDate)
        {
            return new TimeMeasure(refDate, 360.0);
        }

        public DateTime RefDate { get; private set;}
        public double this[DateTime date]
        {
            get
            {
                return (date - RefDate).TotalDays / yearBasis;
            }
        }
        public double[] this[DateTime[] dates]
        {
            get
            {
                var result = new double[dates.Length];
                for (int i = 0; i < result.Length; ++i)
                    result[i] = (dates[i] - RefDate).TotalDays / yearBasis;
                return result;
            }
        }
    }
}
