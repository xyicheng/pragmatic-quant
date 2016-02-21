using System;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;

namespace pragmatic_quant_com.Factories
{
    public static class ObjectConverters
    {
        public static DateTime ConvertDate(object dateObj, DateTime refDate)
        {
            var dateOrDuration = DateAndDurationConverter.ConvertDateOrDuration(dateObj);
            return dateOrDuration.ToDate(refDate);
        }
        public static DateTime[] ConvertDateArray(object[] dateObjs, DateTime refDate)
        {
            return dateObjs.Map(o => ConvertDate(o, refDate));
        }
        public static DateTime[,] ConvertDateArray(object[,] dateObjs, DateTime refDate)
        {
            return dateObjs.Map(o => ConvertDate(o, refDate));
        }
    }
}