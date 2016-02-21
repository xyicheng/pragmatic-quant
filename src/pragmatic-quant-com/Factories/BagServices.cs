using System;
using System.Collections.Generic;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Basic.Structure;

namespace pragmatic_quant_com.Factories
{
    using TimeMatrixDatas = LabelledMatrix<DateOrDuration, string, double>;
    using EqtyVolMatrix = LabelledMatrix<DateOrDuration, double, double>; 

    public static class BagServices
    {
        #region private methods
        private static bool IsEmptyCell(object cellValue)
        {
            if (cellValue == null)
                return true;
            
            var cellAsString = cellValue as string;
            if ((cellAsString != null) && cellAsString.Trim() == "")
                return true;

            if (cellValue is ExcelDna.Integration.ExcelEmpty
                || cellValue is ExcelDna.Integration.ExcelError)
                return true;

            return false;
        }
        private static Exception MissingParameter(string name)
        {
            return new Exception(String.Format("Missing parameter : {0} !", name));
        }
        private static string MissingValueMsg(string name)
        {
            return String.Format("Missing parameter value for {0} !", name);
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

        public static bool Has(this object[,] bag, string name, out int row, out int col)
        {
            var loweredName = name.ToLowerInvariant().Trim();
            for (col = bag.GetLowerBound(1); col <= bag.GetUpperBound(1); ++col)
            {
                for (row = bag.GetLowerBound(0); row <= bag.GetUpperBound(0); ++row)
                {
                    var val = bag[row, col];
                    var valAsString = val as string;
                    if (valAsString != null && valAsString.Trim().Equals(loweredName, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }
            row = int.MaxValue;
            col = int.MaxValue;
            return false;
        }
        public static bool Has(this object[,] bag, string name)
        {
            int row, col;
            return Has(bag, name, out row, out col);
        }

        public static T ProcessScalar<T>(this object[,] bag, int row, int col, Func<object, T> valueMap, string missingException)
        {
            if (col >= bag.GetUpperBound(1))
                throw new Exception(missingException);

            var scalar = bag[row, col + 1];

            if (IsEmptyCell(scalar))
                throw new Exception(missingException);

            return valueMap(scalar);
        }
        public static T ProcessScalar<T>(this object[,] bag, string name, Func<object, T> valueMap)
        {
            int row, col;
            if (Has(bag, name, out row, out col))
            {
                return ProcessScalar(bag, row, col, valueMap, MissingValueMsg(name));
            }
            throw MissingParameter(name);
        }
        public static double ProcessScalarDouble(this object[,] bag, string name)
        {
            return ProcessScalar(bag, name, DoubleValueConverter(name));
        }
        public static int ProcessScalarInteger(this object[,] bag, string name)
        {
            return (int) bag.ProcessScalarDouble(name);
        }
        public static bool ProcessScalarBoolean(this object[,] bag, string name)
        {
            return ProcessScalar(bag, name, o => (bool) o);
        }
        public static string ProcessScalarString(this object[,] bag, string name, bool trim = true)
        {
            return ProcessScalar(bag, name, o => trim ? o.ToString().Trim() : o.ToString());
        }
        public static DateOrDuration ProcessScalarDateOrDuration(this object[,] bag, string name)
        {
            return ProcessScalar(bag, name, DateOrDurationValueConverter(name));
        }

        public static T[] ProcessVector<T>(this object[,] bag, int baseRow, int baseCol, Func<object, T> valueMap, string missingException)
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
        public static T[] ProcessVector<T>(this object[,] bag, string name, Func<object, T> valueMap)
        {
            int row, col;
            if (Has(bag, name, out row, out col))
            {
                return ProcessVector(bag, row + 1, col, valueMap, MissingValueMsg(name));
            }
            throw MissingParameter(name);
        }
        public static double[] ProcessVectorDouble(this object[,] bag, string name)
        {
            return ProcessVector(bag, name, DoubleValueConverter(name));
        }
        public static string[] ProcessVectorString(this object[,] bag, string name)
        {
            return ProcessVector(bag, name, o => o.ToString());
        }
        public static DateOrDuration[] ProcessVectorDateOrDuration(this object[,] bag, string name)
        {
            return ProcessVector(bag, name, DateOrDurationValueConverter(name));
        }

        public static T[,] ProcessMatrix<T>(this object[,] bag, int baseRow, int baseCol, Func<object, T> valueMap, string missingException)
        {
            if (baseRow > bag.GetUpperBound(0))
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
        public static T[,] ProcessMatrix<T>(this object[,] bag, string name, Func<object, T> valueMap)
        {
            int row, col;
            if (Has(bag, name, out row, out col))
            {
                return ProcessMatrix(bag, row + 1, col, valueMap, MissingValueMsg(name));
            }
            throw MissingParameter(name);
        }
        public static double[,] ProcessMatrixDouble(this object[,] bag, string name)
        {
            return ProcessMatrix(bag, name, DoubleValueConverter(name));
        }
        public static string[,] ProcessMatrixString(this object[,] bag, string name)
        {
            return ProcessMatrix(bag, name, o => o.ToString());
        }

        public static LabelledMatrix<TRow, TCol, TVal> ProcessLabelledMatrix<TRow, TCol, TVal>(this object[,] bag, string name,
            Func<object, TRow> rowLabelMap, Func<object, TCol> colLabelMap, Func<object, TVal> valueMap)
        {
            int row, col;
            if (Has(bag, name, out row, out col))
            {
                var valuesAndRowLabel = ProcessMatrix(bag, row + 1, col, o => o, MissingValueMsg(name));

                var rowLabels = new TRow[valuesAndRowLabel.GetLength(0)];
                for (int i = 0; i < rowLabels.Length; i++)
                {
                    rowLabels[i] = rowLabelMap(valuesAndRowLabel[i, 0]);
                }

                var values = valuesAndRowLabel
                    .SubArray(0, valuesAndRowLabel.GetLength(0), 1, valuesAndRowLabel.GetLength(1) - 1)
                    .Map(valueMap);

                var labels = bag
                    .SubArray(row, 1, col + 1, valuesAndRowLabel.GetLength(1) - 1)
                    .Row(0)
                    .Map(colLabelMap);

                return new LabelledMatrix<TRow, TCol, TVal>(rowLabels, labels, values);
            }
            throw MissingParameter(name);
        }
        public static TimeMatrixDatas ProcessTimeMatrixDatas(this object[,] bag, string name)
        {
            return ProcessLabelledMatrix(bag, name, DateOrDurationValueConverter(name), o => o.ToString(), DoubleValueConverter(name));
        }
        public static EqtyVolMatrix ProcessEqtyVolMatrix(this object[,] bag, string name)
        {
            return ProcessLabelledMatrix(bag, name, DateOrDurationValueConverter(name), DoubleValueConverter(name), DoubleValueConverter(name));
        }
    }
}
