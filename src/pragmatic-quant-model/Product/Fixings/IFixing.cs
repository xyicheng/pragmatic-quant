using System;
using System.Diagnostics;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Product.Fixings
{
    public interface IFixing : IObservation
    {
    }

    [DebuggerDisplay("Equity Spot {AssetId} @ {Date}")]
    public class EquitySpot : IFixing
    {
        #region private method
        protected bool Equals(EquitySpot other)
        {
            return Equals(AssetId, other.AssetId) && Date.Equals(other.Date);
        }
        #endregion
        public EquitySpot(DateTime date, AssetId assetId)
        {
            AssetId = assetId;
            Date = date;
        }

        public AssetId AssetId { get; private set; }
        public DateTime Date { get; private set; }
        public Currency Currency
        {
            get { return AssetId.Currency; }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((EquitySpot) obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return ((AssetId != null ? AssetId.GetHashCode() : 0) * 397) ^ Date.GetHashCode();
            }
        }
    }

    [DebuggerDisplay("Libor {Currency} {Tenor} @ {Date}")]
    public class Libor : IFixing
    {
        #region private fields
        protected bool Equals(Libor other)
        {
            return Date.Equals(other.Date) 
                && Equals(Currency, other.Currency) 
                && Equals(Tenor, other.Tenor) 
                && Equals(Schedule, other.Schedule) 
                && Equals(Basis, other.Basis);
        }
        #endregion
        public Libor(Currency currency, Duration tenor, CouponSchedule schedule, DayCountFrac basis)
        {
            Currency = currency;
            Tenor = tenor;
            Schedule = schedule;
            Basis = basis;
            Date = schedule.Fixing;
        }

        public DateTime Date { get; private set; }
        public Currency Currency { get; private set; }
        public Duration Tenor { get; private set; }
        public CouponSchedule Schedule { get; private set; }
        public DayCountFrac Basis { get; private set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Libor) obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Date.GetHashCode();
                hashCode = (hashCode * 397) ^ (Currency != null ? Currency.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Tenor != null ? Tenor.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Schedule != null ? Schedule.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Basis != null ? Basis.GetHashCode() : 0);
                return hashCode;
            }
        }
    }

    public class Zc : IFixing
    {
        #region private fields
        protected bool Equals(Zc other)
        {
            return Date.Equals(other.Date) && PayDate.Equals(other.PayDate) && Equals(Currency, other.Currency) && Equals(FinancingId, other.FinancingId);
        }
        #endregion
        public Zc(DateTime date, DateTime payDate, Currency currency, FinancingId financingId)
        {
            FinancingId = financingId;
            Currency = currency;
            PayDate = payDate;
            Date = date;
        }
        
        public DateTime Date { get; private set; }
        public DateTime PayDate { get; private set; }
        public Currency Currency { get; private set; }
        public FinancingId FinancingId { get; private set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Zc) obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Date.GetHashCode();
                hashCode = (hashCode * 397) ^ PayDate.GetHashCode();
                hashCode = (hashCode * 397) ^ (Currency != null ? Currency.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (FinancingId != null ? FinancingId.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}