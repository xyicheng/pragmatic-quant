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
    }
}
