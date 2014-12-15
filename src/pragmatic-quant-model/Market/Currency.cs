﻿namespace pragmatic_quant_model.Market
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
