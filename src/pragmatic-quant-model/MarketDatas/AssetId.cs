using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.MarketDatas
{
    public class AssetId
    {
        #region private fields
        private readonly string name;
        private readonly Currency currency;
        #endregion
        #region private method
        protected bool Equals(AssetId other)
        {
            return string.Equals(name, other.name) && Equals(currency, other.currency);
        }
        #endregion

        public AssetId(string name, Currency currency)
        {
            this.name = name;
            this.currency = currency;
        }
        public string Name
        {
            get { return name; }
        }
        public Currency Currency
        {
            get { return currency; }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((AssetId)obj);
        }
        public override int GetHashCode()
        {
            unchecked
            {
                return ((name != null ? name.GetHashCode() : 0) * 397) ^ (currency != null ? currency.GetHashCode() : 0);
            }
        }
        public override string ToString()
        {
            return name;
        }
    }
}