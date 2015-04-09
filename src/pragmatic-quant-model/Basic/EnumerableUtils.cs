using System;
using System.Collections.Generic;
using System.Linq;

namespace pragmatic_quant_model.Basic
{
    public static class EnumerableUtils
    {
        public static bool IsSorted<T>(IEnumerable<T> list) where T : IComparable<T>
        {
            var array = list as T[] ?? list.ToArray();
            var y = array.First();
            return array.Skip(1).All(x =>
            {
                bool b = y.CompareTo(x) < 0;
                y = x;
                return b;
            });
        }
        public static T[] ConstantArray<T>(T value, int size)
        {
            var result = new T[size];
            for (int i = 0; i < result.Length; ++i)
                result[i] = value;
            return result;
        }
    }

    public static class FuncUtils
    {
        public static TOut[] Map<TIn, TOut>(this IEnumerable<TIn> list, Func<TIn, TOut> map)
        {
            return list.Select(map).ToArray();
        }
        public static TOut[] ZipWith<TIn1, TIn2, TOut>(this IList<TIn1> left, IList<TIn2> right, Func<TIn1, TIn2, TOut> zipper)
        {
            return left.Select((leftElem, i) => zipper(leftElem, right[i])).ToArray();
        }
        public static TOut[] Scan<TIn, TOut>(this IEnumerable<TIn> list, TOut seed ,Func<TOut, TIn, TOut> aggregator)
        {
            var result = new List<TOut>();
            var previous = seed;
            foreach (var elem in list)
            {
                var current = aggregator(previous, elem);
                result.Add(current);
                previous = current;
            }
            return result.ToArray();
        }
        public static TOut Fold<TIn, TOut>(this IEnumerable<TIn> list, TOut seed, Func<TOut, TIn, TOut> aggregator)
        {
            return list.Aggregate(seed, aggregator);
        }
    }
}
