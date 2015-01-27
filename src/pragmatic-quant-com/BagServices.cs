using System;
using System.Collections.Generic;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Structure;

namespace pragmatic_quant_com
{
    public class BagServices
    {
        #region private methods
        private static Exception MissingParameter(string name)
        {
            return new Exception("Missing parameter : " + name + " !");
        }
        private static Exception MissingValue(string name)
        {
            return new Exception("Missing parameter value for " + name + " !");
        }
        private static Exception MissingOrOutsideValue(string name)
        {
            return new Exception("Parameter value for " + name + " is outside bag or missing !");
        }
        private static bool IsEmptyCell(object cellValue)
        {
            if (cellValue == null)
                return true;

            if ((cellValue is string) && ((string)cellValue).Trim() == "")
                return true;

            return false;
        }
        private static Func<object, double> DoubleValueConverter(string name)
        {
            Func<object, double> doubleConverter = o =>
            {
                double result;
                if (DoubleConverter.TryConvert(o, out result)) return result;
                throw new Exception("Parameter value for " + name + " is not a Double !");
            };
            return doubleConverter;
        }
        private static Func<object, DateOrDuration> DateOrDurationValueConverter(string name)
        {
            Func<object, DateOrDuration> converter = o =>
            {
                DateOrDuration result;
                if (DateAndDurationConverter.TryConvertDateOrDuration(o, out result)) return result;
                throw new Exception("Parameter value for " + name + " is not a Date or a Duration !");
            };
            return converter;
        }
        #endregion
        public static bool Has(object[,] bag, string name, out int row, out int col)
        {
            var loweredName = name.ToLowerInvariant().Trim();

            for (col = bag.GetLowerBound(1); col <= bag.GetUpperBound(1); ++col)
            {
                for (row = bag.GetLowerBound(0); row <= bag.GetUpperBound(0); ++row)
                {
                    var val = bag[row, col];
                    var valAsString = val as string;
                    if (valAsString != null && valAsString.ToLowerInvariant().Trim().Equals(loweredName))
                        return true;
                }
            }
            row = int.MaxValue;
            col = int.MaxValue;
            return false;
        }
        public static bool Has(object[,] bag, string name)
        {
            int row, col;
            return Has(bag, name, out row, out col);
        }

        public static T ProcessScalar<T>(object[,] bag, string name, Func<object, T> valueMap)
        {
            int row, col;
            if (Has(bag, name, out row, out col))
            {
                if (col >= bag.GetUpperBound(1))
                    throw MissingOrOutsideValue(name);

                var scalar = bag[row, col + 1];

                if (IsEmptyCell(scalar))
                    throw MissingValue(name);

                return valueMap(scalar);
            }
            throw MissingParameter(name);
        }
        public static double ProcessScalarDouble(object[,] bag, string name)
        {
            return ProcessScalar(bag, name, DoubleValueConverter(name));
        }
        public static string ProcessScalarString(object[,] bag, string name)
        {
            return ProcessScalar(bag, name, o => o.ToString());
        }
        public static DateOrDuration ProcessScalarDateOrDuration(object[,] bag, string name)
        {
            return ProcessScalar(bag, name, DateOrDurationValueConverter(name));
        }

        public static T[] ProcessVector<T>(object[,] bag, string name, Func<object, T> valueMap)
        {
            int row, col;
            if (Has(bag, name, out row, out col))
            {
                if (row >= bag.GetUpperBound(0))
                    throw MissingOrOutsideValue(name);

                var values = new List<T>();
                for (row = row + 1; row <= bag.GetUpperBound(0); ++row)
                {
                    var value = bag[row, col];
                    if (IsEmptyCell(value)) break;
                    values.Add(valueMap(value));
                }

                if (values.Count == 0)
                    throw MissingValue(name);

                return values.ToArray();
            }
            throw MissingParameter(name);
        }
        public static double[] ProcessVectorDouble(object[,] bag, string name)
        {
            return ProcessVector(bag, name, DoubleValueConverter(name));
        }
        public static string[] ProcessVectorString(object[,] bag, string name)
        {
            return ProcessVector(bag, name, o => o.ToString());
        }
        public static DateOrDuration[] ProcessVectorDateOrDuration(object[,] bag, string name)
        {
            return ProcessVector(bag, name, DateOrDurationValueConverter(name));
        }
    } 
}
