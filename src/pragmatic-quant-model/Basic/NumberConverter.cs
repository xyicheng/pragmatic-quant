using System.Globalization;

namespace pragmatic_quant_model.Basic
{
    public static class NumberConverter
    {
        public static bool TryConvertDouble(object o, out double result)
        {
            if (o is double)
            {
                result = (double)o;
                return true;
            }
            return double.TryParse(o.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }
        public static bool TryConvertInteger(object o, out int result)
        {
            if (o is int)
            {
                result = (int) o;
                return true;
            }

            return int.TryParse(o.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }
    }
}
