using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace pragmatic_quant_model.Basic.Dates
{
    public static class ScheduleUtils
    {
        #region private methods
        private static DateTime[] FrontStubRawSchedule(DateTime start, DateTime end, Duration perio)
        {
            Contract.Requires(end > start);
            Contract.Requires(perio!=null && !perio.IsZero());

            var schedule = new List<DateTime>();
            DateTime current = end;
            while (current > start)
            {
                schedule.Add(current);
                current -= perio;
            }
            schedule.Sort();
            return schedule.ToArray();
        }
        private static DateTime[] BackStubRawSchedule(DateTime start, DateTime end, Duration perio)
        {
            Contract.Requires(end > start);
            Contract.Requires(perio != null && !perio.IsZero());

            var schedule = new List<DateTime>();
            DateTime current = start;
            while (current < end)
            {
                schedule.Add(current);
                current += perio;
            }
            return schedule.ToArray();
        }
        #endregion

        public static DateTime[] RawSchedule(DateTime start, DateTime end, Duration perio, bool frontStub)
        {
            if (start >= end)
                throw new ArgumentException("start must be previous to end");
            if (perio == null || perio.IsZero())
                throw new ArgumentException("perio must be positive");

            return (frontStub) ? FrontStubRawSchedule(start, end, perio) : BackStubRawSchedule(start, end, perio);
        }
    }
}
