using System;
using System.Diagnostics;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.MarketDatas
{
    [DebuggerDisplay("PaymentInfo : Currency = {Currency}, Date = {Date} ...")]
    public class PaymentInfo
    {
        #region private methods
        protected bool Equals(PaymentInfo other)
        {
            return Equals(Currency, other.Currency) && Date.Equals(other.Date) && Equals(Financing, other.Financing);
        }
        #endregion
        public PaymentInfo(Currency currency, DateTime date, FinancingId financing)
        {
            Financing = financing;
            Date = date;
            Currency = currency;
        }
        public PaymentInfo(Currency currency, DateTime date)
            : this(currency, date, FinancingId.RiskFree(currency))
        {
        }

        public Currency Currency { get; private set; }
        public DateTime Date { get; private set; }
        public FinancingId Financing { get; private set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PaymentInfo) obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Currency != null ? Currency.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ Date.GetHashCode();
                hashCode = (hashCode * 397) ^ (Financing != null ? Financing.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}