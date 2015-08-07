using System;
using System.Diagnostics;

namespace pragmatic_quant_model.Basic.Dates
{
    public abstract class DayCountFrac
    {
        public abstract double Count(DateTime start, DateTime end);

        public static readonly DayCountFrac Act365 = new ActDayCountFrac(365.0);
        public static readonly DayCountFrac Act360 = new ActDayCountFrac(360.0);
        public static DayCountFrac Parse(string convention)
        {
            switch (convention.ToLowerInvariant())
            {
                case "act/365" :
                    return Act365;
                case "act/360" :
                    return Act360;
            }
            throw new ArgumentException(string.Format("Unknown DayCount convention {0}", convention));
        }
        
        #region private class
        [DebuggerDisplay("Act/{yearBasis}")]
        private class ActDayCountFrac : DayCountFrac
        {
            private readonly double yearBasis;
            public ActDayCountFrac(double yearBasis)
            {
                this.yearBasis = yearBasis;
            }
            public override double Count(DateTime start, DateTime end)
            {
                return (end - start).TotalDays / yearBasis;
            }
        }
        #endregion
    }
}