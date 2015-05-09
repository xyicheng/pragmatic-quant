using System;

namespace pragmatic_quant_model.Basic.Dates
{
    public class DateOrDuration
    {
        #region private fields
        private readonly Duration duration;
        private readonly DateTime? date;
        private readonly bool isDate;
        #endregion
        public DateOrDuration(DateTime date)
        {
            duration = null;
            this.date = date;
            isDate = true;
        }
        public DateOrDuration(Duration duration)
        {
            this.duration = duration;
            date = null;
            isDate = false;
        }

        public Duration Duration
        {
            get
            {
                return duration;
            }
        }
        public DateTime Date
        {
            get
            {
                if (date != null)
                    return date.Value;
                throw new Exception("Not a date !");
            }
        }
        public bool IsDuration
        {
            get
            {
                return !isDate;
            }
        }
        public bool IsDate
        {
            get
            {
                return isDate;
            }
        }

        public DateTime ToDate(DateTime refDate)
        {
            if (isDate && date != null) return date.Value;
            return refDate + duration;
        }
    }
}