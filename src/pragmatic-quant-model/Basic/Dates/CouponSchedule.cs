using System;

namespace pragmatic_quant_model.Basic.Dates
{
    public class CouponSchedule
    {
        public CouponSchedule(DateTime fixing, DateTime start, DateTime end, DateTime pay)
        {
            Fixing = fixing;
            Start = start;
            End = end;
            Pay = pay;
        }

        public readonly DateTime Fixing;
        public readonly DateTime Start;
        public readonly DateTime End;
        public readonly DateTime Pay;
    }
}