using System.Globalization;

namespace pragmatic_quant_model.Basic
{
    public static class DoubleConverter
    {
        public static bool TryConvert(object o, out double result)
        {
            if (o is double)
            {
                result = (double)o;
                return true;
            }
            return double.TryParse(o.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out result);
        }
    }
}
