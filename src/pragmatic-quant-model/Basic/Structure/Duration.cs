
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace pragmatic_quant_model.Basic.Structure
{
    public class Duration
    {
        #region private fields
        private readonly BasicDuration unity;
        private readonly int nbunit;
        #endregion
        #region private methods
        private Duration(BasicDuration unity, int nbunit)
        {
            this.unity = unity;
            this.nbunit = nbunit;
        }
        #endregion
        public override string ToString()
        {
            switch (unity)
            {
                case BasicDuration.Day:
                    return nbunit.ToString(CultureInfo.InvariantCulture) + "D";
                case BasicDuration.Month:
                    return nbunit.ToString(CultureInfo.InvariantCulture) + "M";
                case BasicDuration.Year:
                    return nbunit.ToString(CultureInfo.InvariantCulture) + "Y";
                default:
                    throw new Exception("Duration : Should not get there !");
            }
        }
        public override bool Equals(object obj)
        {
            var asDur = obj as Duration;
            if (asDur == null)
                return false;
            //Todo : Gerer l'egalité 12m=1y
            return (unity == asDur.unity && nbunit == asDur.nbunit);
        }
        public override int GetHashCode()
        {
            return unity.GetHashCode() ^ nbunit.GetHashCode();
        }

        public static readonly Duration Year = new Duration(BasicDuration.Year, 1);
        public static readonly Duration Month = new Duration(BasicDuration.Month, 1);
        public static readonly Duration Day = new Duration(BasicDuration.Day, 1);

        public static DateTime operator +(DateTime date, Duration dur)
        {
            switch (dur.unity)
            {
                case BasicDuration.Day:
                    return date.AddDays(dur.nbunit);
                case BasicDuration.Month:
                    return date.AddMonths(dur.nbunit);
                case BasicDuration.Year:
                    return date.AddYears(dur.nbunit);
                default:
                    throw new Exception("Duration : Should not get there !");
            }
        }
        public static Duration operator *(Duration date, int mult)
        {
            return new Duration(date.unity, date.nbunit * mult);
        }
        public static Duration operator *(int mult, Duration date)
        {
            return new Duration(date.unity, date.nbunit * mult);
        }
        public static bool TryParse(string s, out Duration result)
        {
            var durRegex = new Regex(@"\d+[dDmMyY]");
            var match = durRegex.Match(s);
            if (match.Success && durRegex.Replace(s, "").Trim() == "")
            {
                var durDesc = match.Value;
                var nbUnitDesc = Regex.Replace(durDesc, "[dDmMyY]", "");
                int nbUnit = int.Parse(nbUnitDesc);

                var unitDesc = durDesc.Replace(nbUnitDesc, "").ToLower();
                BasicDuration unit;
                switch (unitDesc)
                {
                    case "d":
                        unit = BasicDuration.Day;
                        break;
                    case "m":
                        unit = BasicDuration.Month;
                        break;
                    case "y":
                        unit = BasicDuration.Year;
                        break;
                    default:
                        throw new Exception("Should never get there !");
                }

                result = new Duration(unit, nbUnit);
                return true;
            }

            result = null;
            return false;
        }

        private enum BasicDuration { Day, Month, Year };
    }

    public class DateOrDuration
    {
        #region private fields
        private readonly Duration duration;
        private readonly DateTime? date;
        private readonly bool isDate;
        #endregion
        public DateOrDuration(DateTime date)
        {
            duration = null;
            this.date = date;
            isDate = true;
        }
        public DateOrDuration(Duration duration)
        {
            this.duration = duration;
            date = null;
            isDate = false;
        }

        public Duration Duration
        {
            get
            {
                return duration;
            }
        }
        public DateTime Date
        {
            get
            {
                if (date != null)
                    return date.Value;
                throw new Exception("Not a date !");
            }
        }
        public bool IsDuration
        {
            get
            {
                return !isDate;
            }
        }
        public bool IsDate
        {
            get
            {
                return isDate;
            }
        }

        public DateTime ToDate(DateTime refDate)
        {
            if (isDate && date != null) return date.Value;
            return refDate + duration;
        }
    }

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
            if (DateTime.TryParse(asString, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out dateParsing))
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
