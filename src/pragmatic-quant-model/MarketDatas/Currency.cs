using System;

namespace pragmatic_quant_model.MarketDatas
{
    public class Currency
    {
        #region private fields
        private readonly string name;
        #endregion
        #region private methods
        private Currency(string name)
        {
            this.name = name;
        }
        private bool Equals(Currency other)
        {
            return string.Equals(name, other.name);
        }
        #endregion
        public string Name
        {
            get { return name; }
        }

        public static readonly Currency Eur = new Currency("EUR");
        public static readonly Currency Usd = new Currency("USD");
        public static readonly Currency Jpy = new Currency("JPY");
        public static bool TryParse(string desc, out Currency currency)
        {
            switch (desc.Trim().ToUpper())
            {
                case "EUR":
                    currency = Eur;
                    break;
                case "USD":
                    currency = Usd;
                    break;
                case "JPY":
                    currency = Jpy;
                    break;
                default:
                    currency = null;
                    return false;
            }
            return true;
        }
        public static Currency Parse(string desc)
        {
            Currency cur;
            if (!TryParse(desc, out cur))
                throw new Exception(string.Format("Unknown currency : {0}", desc));
            return cur;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Currency)obj);
        }
        public override int GetHashCode()
        {
            return (name != null ? name.GetHashCode() : 0);
        }
        public override string ToString()
        {
            return name;
        }
    }
}
