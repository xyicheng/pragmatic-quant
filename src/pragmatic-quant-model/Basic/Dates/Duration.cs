using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Text.RegularExpressions;

namespace pragmatic_quant_model.Basic.Dates
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
        private static DateTime Add(DateTime date, BasicDuration unity, int nbunit)
        {
            switch (unity)
            {
                case BasicDuration.Hour:
                    return date.AddHours(nbunit);
                case BasicDuration.Day:
                    return date.AddDays(nbunit);
                case BasicDuration.Month:
                    return date.AddMonths(nbunit);
                case BasicDuration.Year:
                    return date.AddYears(nbunit);
                default:
                    throw new Exception("Duration : Should not get there !");
            }
        }
        #endregion
        private enum BasicDuration { Hour, Day, Month, Year };
        
        public static readonly Duration Year = new Duration(BasicDuration.Year, 1);
        public static readonly Duration Month = new Duration(BasicDuration.Month, 1);
        public static readonly Duration Day = new Duration(BasicDuration.Day, 1);
        public static readonly Duration Hour = new Duration(BasicDuration.Hour, 1);

        public static DateTime operator +(DateTime date, Duration dur)
        {
            return Add(date, dur.unity, dur.nbunit);
        }
        public static DateTime operator -(DateTime date, Duration dur)
        {
            return Add(date, dur.unity, -dur.nbunit);
        }
        public static Duration operator *(Duration date, int mult)
        {
            Contract.Requires(mult >= 0);
            return new Duration(date.unity, date.nbunit * mult);
        }
        public static Duration operator *(int mult, Duration date)
        {
            Contract.Requires(mult >= 0);
            return new Duration(date.unity, date.nbunit * mult);
        }
        
        public static bool TryParse(string s, out Duration result)
        {
            var durRegex = new Regex(@"\d+[hHdDmMyY]");
            var match = durRegex.Match(s);
            if (match.Success && durRegex.Replace(s, "").Trim() == "")
            {
                var durDesc = match.Value;
                var nbUnitDesc = Regex.Replace(durDesc, "[hHdDmMyY]", "");
                int nbUnit = int.Parse(nbUnitDesc);

                var unitDesc = durDesc.Replace(nbUnitDesc, "").ToLowerInvariant();
                BasicDuration unit;
                switch (unitDesc)
                {
                    case "h":
                        unit = BasicDuration.Hour;
                        break;
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
        public static Duration Parse(string s)
        {
            Duration d;
            if (TryParse(s, out d))
                return d;
            throw new Exception("Failed to parse duration : " + s);
        }

        public bool IsZero()
        {
            return nbunit == 0;
        }

        public override string ToString()
        {
            switch (unity)
            {
                case BasicDuration.Hour:
                    return nbunit.ToString(CultureInfo.InvariantCulture) + "H";
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
            //Todo : 12m = 1y and 24h = 1d 
            return (unity == asDur.unity && nbunit == asDur.nbunit);
        }
        public override int GetHashCode()
        {
            return unity.GetHashCode() ^ nbunit.GetHashCode();
        }
    }
}
