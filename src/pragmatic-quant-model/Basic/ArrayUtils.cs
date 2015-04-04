using System;

namespace pragmatic_quant_model.Basic
{
    public static class ArrayUtils
    {
        public static T[] ExtractColumn<T>(this T[,] array, int index)
        {
            if (index < 0 || index >= array.GetLength(1))
                throw new IndexOutOfRangeException();

            var col = new T[array.GetLength(0)];
            for (int i = 0; i < col.Length; i++)
                col[i] = array[i, index];
            return col;
        }
        public static T[] ExtractRow<T>(this T[,] array, int index)
        {
            if (index < 0 || index >= array.GetLength(0))
                throw new IndexOutOfRangeException();

            var col = new T[array.GetLength(1)];
            for (int j = 0; j < col.Length; j++)
                col[j] = array[index, j];
            return col;
        }
        public static T[,] ExtractSubArray<T>(this T[,] array, int rowStartIndex, int rowLength, 
                                                               int colStartIndex, int colLength)
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
        public static T[] ExtractSubArray<T>(this T[] array, int startIndex, int length)
        {
            if (startIndex < 0 || startIndex + length > array.Length)
                throw new IndexOutOfRangeException();
            var result = new T[length];
            for (int i = 0; i < result.Length; i++)
                result[i] = array[startIndex + i];
            return result;
        }

        public static TB[,] Map<TA, TB>(this TA[,] array, Func<TA, TB> map)
        {
            var result = new TB[array.GetLength(0), array.GetLength(1)];
            for(int i=0; i<result.GetLength(0); i++)
                for (int j = 0; j < result.GetLength(1); j++)
                    result[i, j] = map(array[i, j]);
            return result;
        }
        public static TB[] Map<TA, TB>(this TA[] array, Func<TA, TB> map)
        {
            var result = new TB[array.Length];
            for (int i = 0; i < result.Length; i++)
                    result[i] = map(array[i]);
            return result;
        }
    }
}
