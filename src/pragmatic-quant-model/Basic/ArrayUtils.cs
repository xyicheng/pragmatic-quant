using System;
using System.Linq;

namespace pragmatic_quant_model.Basic
{
    public static class ArrayUtils
    {
        public static T[] Column<T>(this T[,] array, int index)
        {
            if (index < 0 || index >= array.GetLength(1))
                throw new IndexOutOfRangeException();

            var col = new T[array.GetLength(0)];
            for (int i = 0; i < col.Length; i++)
                col[i] = array[i, index];
            return col;
        }
        public static T[] Row<T>(this T[,] array, int index)
        {
            if (index < 0 || index >= array.GetLength(0))
                throw new IndexOutOfRangeException();

            var col = new T[array.GetLength(1)];
            for (int j = 0; j < col.Length; j++)
                col[j] = array[index, j];
            return col;
        }
        public static T[,] SubArray<T>(this T[,] array, int rowStartIndex, int rowLength, int colStartIndex, int colLength)
        {
            if (rowStartIndex < 0 || rowStartIndex + rowLength > array.GetLength(0))
                throw new IndexOutOfRangeException();
            if (colStartIndex < 0 || colStartIndex + colLength > array.GetLength(1))
                throw new IndexOutOfRangeException();

            var result = new T[rowLength, colLength];
            for (int i = 0; i < result.GetLength(0); i++)
                for (int j = 0; j < result.GetLength(1); j++)
                    result[i, j] = array[rowStartIndex + i, colStartIndex + j];
            return result;
        }
        public static T[] SubArray<T>(this T[] array, int startIndex, int length)
        {
            if (startIndex < 0 || startIndex + length > array.Length)
                throw new IndexOutOfRangeException();
            var result = new T[length];
            for (int i = 0; i < result.Length; i++)
                result[i] = array[startIndex + i];
            return result;
        }
        public static TResult[,] Map<T, TResult>(this T[,] array, Func<T, TResult> map)
        {
            var result = new TResult[array.GetLength(0), array.GetLength(1)];
            for (int i = 0; i < result.GetLength(0); i++)
                for (int j = 0; j < result.GetLength(1); j++)
                    result[i, j] = map(array[i, j]);
            return result;
        }
        public static TResult[,] CartesianProd<TA, TB, TResult>(TA[] a, TB[] b, Func<TA, TB, TResult> func)
        {
            var result = new TResult[a.Count(), b.Count()];
            for (int i = 0; i < a.Count(); i++)
                for (int j = 0; j < b.Count(); j++)
                    result[i, j] = func(a[i], b[j]);
            return result;
        }
        
        public static void SetCol<T>(ref T[,] array, int colIndex, T[] col)
        {
            if (colIndex < 0 || colIndex >= array.GetLength(1))
                throw new IndexOutOfRangeException();
            if (col.Length != array.GetLength(0))
                throw new IndexOutOfRangeException();

            for (int i = 0; i < col.Length; i++)
                array[i, colIndex] = col[i];
        }
        public static void SetRow<T>(ref T[,] array, int rowIndex, T[] row)
        {
            if (rowIndex < 0 || rowIndex >= array.GetLength(0))
                throw new IndexOutOfRangeException();
            if (row.Length != array.GetLength(1))
                throw new IndexOutOfRangeException();

            for (int i = 0; i < row.Length; i++)
                array[rowIndex, i] = row[i];
        }
        public static void SetSubArray<T>(ref T[,] array, int rowIndex, int colIndex, T[,] subArray)
        {
            if (rowIndex < 0 || rowIndex >= array.GetLength(0)
                || colIndex < 0 || colIndex >= array.GetLength(1))
                throw new IndexOutOfRangeException();
            if (rowIndex + subArray.GetLength(0) > array.GetLength(0)
                || colIndex + subArray.GetLength(1) > array.GetLength(1))
                throw new IndexOutOfRangeException();

            for (int i = 0; i < subArray.GetLength(0); i++)
                for (int j = 0; j < subArray.GetLength(1); j++)
                    array[rowIndex + i, colIndex + j] = subArray[i, j];
        }

        public static T[,] AsColumn<T>(this T[] col)
        {
            var result = new T[col.Length, 1];
            SetCol(ref result, 0, col);
            return result;
        }
        public static T[,] AsRow<T>(this T[] row)
        {
            var result = new T[1, row.Length];
            SetRow(ref result, 0, row);
            return result;
        }
        public static T[][] CreateJaggedArray<T>(int nbRows, int nbCols)
        {
            var array = new T[nbRows][];
            for (int i = 0; i < nbRows; i++)
                array[i] = new T[nbCols];
            return array;
        }

        public static int FindIndex<T>(this T[] array, T element)
        {
            return array.ToList().FindIndex(e => e.Equals(element));
        }
    }
}
