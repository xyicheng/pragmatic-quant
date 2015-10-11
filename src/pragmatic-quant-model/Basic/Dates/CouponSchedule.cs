using System;

namespace pragmatic_quant_model.Basic.Dates
{
    public class CouponSchedule
    {
        #region private fields
        protected bool Equals(CouponSchedule other)
        {
            return Fixing.Equals(other.Fixing) && Start.Equals(other.Start) && End.Equals(other.End) && Pay.Equals(other.Pay);
        }
        #endregion
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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((CouponSchedule) obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Fixing.GetHashCode();
                hashCode = (hashCode * 397) ^ Start.GetHashCode();
                hashCode = (hashCode * 397) ^ End.GetHashCode();
                hashCode = (hashCode * 397) ^ Pay.GetHashCode();
                return hashCode;
            }
        }
    }
}