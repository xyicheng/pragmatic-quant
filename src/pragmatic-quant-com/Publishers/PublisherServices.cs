using System;
using System.Collections.Generic;
using System.Linq;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_com.Publishers
{
    public static class PublisherServices
    {
        #region private fields
        private static readonly object emptyCellValue = ""; //ExcelDna.Integration.ExcelEmpty.Value
        #endregion
        
        public static object[,] PublishScalar(string name, object value)
        {
            return new [,] {{name, value}};
        }
        public static object[,] PublishDictionary<TA, TB>(string name, IDictionary<TA, TB> dico)
        {
            var result = new object[dico.Count + 1, 2];
            result[0, 0] = name;
            result[0, 1] = emptyCellValue;

            var keys = dico.Keys.ToArray();
            var values = dico.Values.ToArray();
            for (int i = 0; i < dico.Count; i++)
            {
                result[1 + i, 0] = keys[i];
                result[1 + i, 1] = values[i];
            }
            return result;
        }

        public static object[,] AppendUnder(this object[,] publish, object[,] under, uint verticalSpace = 0)
        {
            var nbRows = publish.GetLength(0) + under.GetLength(0) + verticalSpace;
            var nbCols = Math.Max(publish.GetLength(1), under.GetLength(1));
            var result = new object[nbRows, nbCols].Map(o => emptyCellValue);
            ArrayUtils.SetSubArray(ref result, 0, 0, publish);
            ArrayUtils.SetSubArray(ref result, publish.GetLength(0) + (int) verticalSpace, 0, under);
            return result;
        }
    }
}