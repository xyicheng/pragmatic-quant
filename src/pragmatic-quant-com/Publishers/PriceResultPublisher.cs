using System;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Pricing;

namespace pragmatic_quant_com.Publishers
{
    public class PriceResultPublisher : Singleton<PriceResultPublisher>, IPublisher<IPricingResult>
    {
        #region private fields
        private static void PricingDetails(Tuple<string, PaymentInfo, Price>[] details,
            out DateTime[] payDates, out Currency[] payCurrencies, out double[,] values)
        {
            payDates = details.Select(pi => pi.Item2.Date)
                .Distinct()
                .OrderBy(d => d)
                .ToArray();
            var payDateIndexes = payDates.ZipToDictionary(Enumerable.Range(0, payDates.Length).ToArray());

            payCurrencies = details.Select(pi => pi.Item2.Currency)
                .Distinct()
                .ToArray();
            var payCurrencyIndexes = payCurrencies.ZipToDictionary(Enumerable.Range(0, payCurrencies.Length).ToArray());

            values = new double[payDates.Length, payCurrencies.Length];
            foreach (var kv in details)
            {
                var pay = kv.Item2;
                int dateIndex = payDateIndexes[pay.Date];
                int currencyIndex = payCurrencyIndexes[pay.Currency];
                values[dateIndex, currencyIndex] += kv.Item3.Value;
            }
        }
        #endregion
        public object[,] PublishPriceResult(PriceResult priceResult)
        {
            var price = priceResult.Price;
            var result = PublisherServices.PublishScalar(string.Format("Price ({0})", price.Currency), price.Value);

            DateTime[] payDates;
            Currency[] payCurrencies;
            double[,] detailValues;
            PricingDetails(priceResult.Details, out payDates, out payCurrencies, out detailValues);

            var details = new object[payDates.Length + 1, payCurrencies.Length + 1];
            details[0, 0] = "PricingDetails";
            ArrayUtils.SetSubArray(ref details, 1, 1, detailValues.Map(v => (object)v));
            ArrayUtils.SetSubArray(ref details, 1, 0, payDates.Cast<object>().ToArray().AsColumn());
            ArrayUtils.SetSubArray(ref details, 0, 1, payCurrencies.Map(c => (object)c.ToString()).AsRow());

            result = result.AppendUnder(details, 1);
            return result;
        }
        
        public object[,] Publish(IPricingResult pricingResult)
        {
            var priceResult = pricingResult as PriceResult;
            if(priceResult!=null)
                return PublishPriceResult(priceResult);

            throw new Exception(string.Format("Pricing result publisher do not handle type {0}", pricingResult.GetType()));
        }
    }
}