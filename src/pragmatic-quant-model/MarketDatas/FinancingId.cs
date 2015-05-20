using System;

namespace pragmatic_quant_model.MarketDatas
{
    public class FinancingId
    {
        #region private fields
        private readonly string id;
        private readonly Currency currency;
        #endregion
        #region protected fields
        protected FinancingId(string id, Currency currency)
        {
            this.id = id;
            this.currency = currency;
        }
        protected bool Equals(FinancingId other)
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
        
        public static FinancingId RiskFree(Currency currency)
        {
            return new FinancingId("RiskFree", currency);
        }
        public static bool TryParse(string curveId, out FinancingId result)
        {
            result = null;

            var splitted = curveId.Split('.');
            if (splitted.Length != 2)
                throw new Exception(string.Format("Not a valid FinancingCurveId : {0}", curveId));
            
            Currency currency;
            if (!Currency.TryParse(splitted[1], out currency))
                return false;
            
            switch (splitted[0].Trim().ToLowerInvariant())
            {
                case "riskfree":
                    result = RiskFree(currency);
                    return true;
            }

            return false;
        }
        public static FinancingId Parse(string curveId)
        {
            FinancingId result;
            if (!TryParse(curveId, out result))
            {
                throw new Exception(string.Format("Unable to parse financing curve id : {0}", curveId));
            }
            return result;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((FinancingId)obj);
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
