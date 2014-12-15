namespace pragmatic_quant_model.Market
{
    public class FinancingCurveId
    {
        #region private fields
        private readonly string id;
        private readonly Currency currency;
        #endregion
        #region protected fields
        protected FinancingCurveId(string id, Currency currency)
        {
            this.id = id;
            this.currency = currency;
        }
        protected bool Equals(FinancingCurveId other)
        {
            return string.Equals(id, other.id) && Equals(currency, other.currency);
        }
        #endregion

        public string Id
        {
            get { return id; }
        }
        public Currency Currency
        {
            get { return currency; }
        }
        public static FinancingCurveId RiskFree(Currency currency)
        {
            return new FinancingCurveId("RiskFree", currency);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FinancingCurveId)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return ((id != null ? id.GetHashCode() : 0) * 397) ^ (currency != null ? currency.GetHashCode() : 0);
            }
        }
        public override string ToString()
        {
            return id + "." + currency.Name;
        }
    }
}
