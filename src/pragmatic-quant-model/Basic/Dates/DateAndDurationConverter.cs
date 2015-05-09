using System;
using System.Globalization;

namespace pragmatic_quant_model.Basic.Dates
{
    public class DateAndDurationConverter
    {
        public static bool TryConvertDate(object o, out DateTime result)
        {
            if (o is DateTime)
            {
                result = (DateTime)o;
                return true;
            }

            var asDateOrDuration = o as DateOrDuration;
            if (asDateOrDuration != null && asDateOrDuration.IsDate)
            {
                result = asDateOrDuration.Date;
                return true;
            }

            if (o is double)
            {
                result = DateTime.FromOADate((double)o);
                return true;
            }

            return DateTime.TryParse(o.ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out result);
        }
        public static bool TryConvertDuration(object o, out Duration result)
        {
            var duration = o as Duration;
            if (duration != null)
            {
                result = duration;
                return true;
            }

            var asDateOrDuration = o as DateOrDuration;
            if (asDateOrDuration != null && asDateOrDuration.IsDuration)
            {
                result = asDateOrDuration.Duration;
                return true;
            }

            return Duration.TryParse(o.ToString(), out result);
        }
        public static bool TryConvertDateOrDuration(object o, out DateOrDuration result)
        {
            var duration = o as DateOrDuration;
            if (duration != null)
            {
                result = duration;
                return true;
            }

            if (o is DateTime)
            {
                result = new DateOrDuration((DateTime)o);
                return true;
            }

            var duration1 = o as Duration;
            if (duration1 != null)
            {
                result = new DateOrDuration(duration1);
                return true;
            }
            if (o is double)
            {
                result = new DateOrDuration(DateTime.FromOADate((double)o));
                return true;
            }

            var asString = o.ToString();

            DateTime dateParsing;
            if (DateTime.TryParse(asString, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dateParsing)) //TODO : check culture
            {
                result = new DateOrDuration(dateParsing);
                return true;
            }

            Duration durationParsing;
            if (Duration.TryParse(asString, out durationParsing))
            {
                result = new DateOrDuration(durationParsing);
                return true;
            }

            result = null;
            return false;
        }

        public static DateTime ConvertDate(object o)
        {
            DateTime result;
            if (!TryConvertDate(o, out result))
                throw new Exception("Failed to convert " + o + " to Date");
            return result;
        }
        public static Duration ConvertDuration(object o)
        {
            Duration result;
            if (!TryConvertDuration(o, out result))
                throw new Exception("Failed to convert " + o + " to Duration");
            return result;
        }
        public static DateOrDuration ConvertDateOrDuration(object o)
        {
            DateOrDuration result;
            if (!TryConvertDateOrDuration(o, out result))
                throw new Exception("Failed to convert " + o + " to DateOrDuration");
            return result;
        }
    }
}