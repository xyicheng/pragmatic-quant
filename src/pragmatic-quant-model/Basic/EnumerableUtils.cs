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
        public static T[] For<T>(int startIndex, int count, Func<int, T> func)
        {
            return Enumerable.Range(startIndex, count).Select(func).ToArray();
        }
        public static T[] Append<T>(params IEnumerable<T>[] lists)
        {
            return lists.Aggregate(new T[0], (result, current) => result.Concat(current).ToArray());
        }
        
        public static T[] Merge<T>(params IEnumerable<T>[] lists)
        {
            return lists.Aggregate(new T[0], (result, current) => result.Union(current).ToArray());
        }
        public static T[] MergeWith<T>(this IEnumerable<T> list, params IEnumerable<T>[] otherLists)
        {
            var mergedOthers = Merge(otherLists);
            return Merge(list, mergedOthers);
        }

        public static IDictionary<TIn, TOut> ZipToDictionary<TIn, TOut>(this IList<TIn> keys, IList<TOut> values)
        {
            return Enumerable.Range(0, keys.Count).ToDictionary(i => keys[i], i => values[i]);
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
