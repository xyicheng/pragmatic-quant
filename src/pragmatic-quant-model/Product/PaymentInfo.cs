using System;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Product
{
    public class PaymentInfo
    {
        public PaymentInfo(Currency currency, DateTime date, FinancingCurveId financing)
        {
            Financing = financing;
            Date = date;
            Currency = currency;
        }

        public Currency Currency { get; private set; }
        public DateTime Date { get; private set; }
        public FinancingCurveId Financing { get; private set; }
    }
}