using System;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Product
{
    public class PaymentInfo
    {

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
    }
}