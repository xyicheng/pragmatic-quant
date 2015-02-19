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
        private readonly DateTime refDate;
        #endregion
        public TimeMeasure(DateTime refDate)
        {
            this.refDate = refDate;
        }
        public static ITimeMeasure Create(DateTime refDate)
        {
            return new TimeMeasure(refDate);
        }

        public DateTime RefDate
        {
            get
            {
                return refDate;
            }
        }
        public double this[DateTime date]
        {
            get
            {
                return (date - refDate).TotalDays / 365.0;
            }
        }
        public double[] this[DateTime[] dates]
        {
            get
            {
                var result = new double[dates.Length];
                for (int i = 0; i < result.Length; ++i)
                    result[i] = (dates[i] - refDate).TotalDays / 365.0;
                return result;
            }
        }
    }
}
