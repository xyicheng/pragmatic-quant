using System;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Product.Fixings
{
    public interface IFixing : IObservation
    {
    }

    public class EquitySpot : IFixing
    {
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
    }

    public class Libor : IFixing
    {
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
    }

    public class Zc : IFixing
    {
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
    }
}