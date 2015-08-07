using ExcelDna.Integration;
using pragmatic_quant_model.Basic.Dates;

namespace pragmatic_quant_com
{
    public class DateFunctions
    {
        [ExcelFunction(Description = "Count time between two days.", Category = "PragmaticQuant_DateFunctions")]
        public static object DayCount(object start, object end,
            [ExcelArgument("Convention used for counting time : Act/365, Act/360...")] string convention)
        {
            var startDate = DateAndDurationConverter.ConvertDate(start);
            var endDate = DateAndDurationConverter.ConvertDate(end);
            var dayCount = DayCountFrac.Parse(convention);
            return dayCount.Count(startDate, endDate);
        }

        [ExcelFunction(Description = "Helper for add duration ('1d','5y'...) to a given date.",
            Category = "PragmaticQuant_DateFunctions")]
        public static object AddDuration(object refDate, object period)
        {
            var date = DateAndDurationConverter.ConvertDate(refDate);
            var duration = DateAndDurationConverter.ConvertDuration(period);
            return date + duration;
        }
    }
}