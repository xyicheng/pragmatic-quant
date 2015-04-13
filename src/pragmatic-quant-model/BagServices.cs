using System;
using System.Collections.Generic;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Structure;

namespace pragmatic_quant_model
{
    public class BagServices
    {
        #region private methods
        private static Exception MissingParameter(string name)
        {
            return new Exception(String.Format("Missing parameter : {0} !", name));
        }
        private static string MissingValueMsg(string name)
        {
            return String.Format("Missing parameter value for {0} !", name);
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
                if (NumberConverter.TryConvertDouble(o, out result)) return result;
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
                throw new Exception(String.Format("Parameter value for {0} is not a Date or a Duration !", name));
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

        public static T ProcessScalar<T>(object[,] bag, int row, int col, Func<object, T> valueMap, string missingException)
        {
            if (col >= bag.GetUpperBound(1))
                throw new Exception(missingException);

            var scalar = bag[row, col + 1];

            if (IsEmptyCell(scalar))
                throw new Exception(missingException);

            return valueMap(scalar);
        }
        public static T ProcessScalar<T>(object[,] bag, string name, Func<object, T> valueMap)
        {
            int row, col;
            if (Has(bag, name, out row, out col))
            {
                return ProcessScalar(bag, row, col, valueMap, MissingValueMsg(name));
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

        public static T[] ProcessVector<T>(object[,] bag, int baseRow, int baseCol, Func<object, T> valueMap, string missingException)
        {
            if (baseRow >= bag.GetUpperBound(0))
                throw new Exception(missingException);

            var values = new List<T>();
            for (int row = baseRow; row <= bag.GetUpperBound(0); ++row)
            {
                var value = bag[row, baseCol];
                if (IsEmptyCell(value)) break;
                values.Add(valueMap(value));
            }

            if (values.Count == 0)
                throw new Exception(missingException);

            return values.ToArray();
        }
        public static T[] ProcessVector<T>(object[,] bag, string name, Func<object, T> valueMap)
        {
            int row, col;
            if (Has(bag, name, out row, out col))
            {
                return ProcessVector(bag, row + 1, col, valueMap, MissingValueMsg(name));
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

        public static T[,] ProcessMatrix<T>(object[,] bag, int baseRow, int baseCol, Func<object, T> valueMap, string missingException)
        {
            if (baseRow >= bag.GetUpperBound(0))
                throw new Exception(missingException);

            int nbRows = 0, nbCols = 0;
            
            for (int row = baseRow; row <= bag.GetUpperBound(0); row++)
            {
                var value = bag[row, baseCol];
                if (IsEmptyCell(value)) break;
                nbRows++;
            }
            if (nbRows == 0)
                throw new Exception(missingException);
            for (int col = baseCol; col <= bag.GetUpperBound(1); col++)
            {
                var value = bag[baseRow, col];
                if (IsEmptyCell(value)) break;
                nbCols++;
            }

            var result = new T[nbRows, nbCols];
            for (int i = 0; i < nbRows; i++)
            {
                for (int j = 0; j < nbCols; j++)
                {
                    result[i, j] = valueMap(bag[baseRow + i, baseCol + j]);
                }
            }
            return result;
        }
        public static T[,] ProcessMatrix<T>(object[,] bag, string name, Func<object, T> valueMap)
        {
            int row, col;
            if (Has(bag, name, out row, out col))
            {
                return ProcessMatrix(bag, row + 1, col, valueMap, MissingValueMsg(name));
            }
            throw MissingParameter(name);
        }
        public static double[,] ProcessMatrixDouble(object[,] bag, string name)
        {
            return ProcessMatrix(bag, name, DoubleValueConverter(name));
        }
        public static string[,] ProcessMatrixString(object[,] bag, string name)
        {
            return ProcessMatrix(bag, name, o => o.ToString());
        }

        public static LabelledMatrix<TRow, TCol, TVal> ProcessLabelledMatrix<TRow, TCol, TVal>(object[,] bag, string name, 
            Func<object, TRow> rowLabelMap,Func<object, TCol> colLabelMap, Func<object, TVal> valueMap)
        {
            int row, col;
            if (Has(bag, name, out row, out col))
            {
                var valuesAndDates = ProcessMatrix(bag, row + 1, col, o => o, MissingValueMsg(name));

                var dates = new TRow[valuesAndDates.GetLength(0)];
                for (int i = 0; i < dates.Length; i++)
                {
                    dates[i] = rowLabelMap(valuesAndDates[i, 0]);
                }

                var values = valuesAndDates
                    .ExtractSubArray(0, valuesAndDates.GetLength(0), 1, valuesAndDates.GetLength(1) - 1)
                    .Map(valueMap);

                var labels = bag
                    .ExtractSubArray(row, 1, col + 1, valuesAndDates.GetLength(1) - 1)
                    .ExtractRow(0)
                    .Map(colLabelMap);

                return new LabelledMatrix<TRow, TCol, TVal>(dates, labels, values);
            }
            throw MissingParameter(name);
        }
        public static TimeMatrixDatas ProcessTimeMatrixDatas(object[,] bag, string name)
        {
            var matrix  = ProcessLabelledMatrix(bag, name, DateOrDurationValueConverter(name), o => o.ToString(), DoubleValueConverter(name));
            return new TimeMatrixDatas(matrix);
        }
    } 
}
