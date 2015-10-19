using System;
using System.Linq;

namespace pragmatic_quant_model.Basic.Structure
{
    public class LabelledMatrix<TRow, TCol, TVal>
    {
        public LabelledMatrix(TRow[] rowLabels, TCol[] colLabels, TVal[,] values)
        {
            if (values.GetLength(0) != rowLabels.Length || values.GetLength(1) != colLabels.Length)
                throw new Exception("TimeMatrixData : invalid format datas");

            Values = values;
            ColLabels = colLabels;
            RowLabels = rowLabels;
        }
        public TRow[] RowLabels { get; private set; }
        public TCol[] ColLabels { get; private set; }
        public TVal[,] Values { get; private set; }
    }

    public static class LabelledMatrixUtils
    {
        public static TVal[] GetColFromLabel<TRow, TVal>(this LabelledMatrix<TRow, string, TVal> matrix, string label)
        {
            var labelIndexes = matrix.ColLabels
                                .Select((l, i) => new {Label = l, Index = i})
                                .Where(data => data.Label.Equals(label, StringComparison.InvariantCultureIgnoreCase))
                                .Select(data => data.Index)
                                .ToArray();

            if (!labelIndexes.Any())
                throw new Exception(string.Format("Missing Label : {0}", label));

            if (labelIndexes.Count() != 1)
                throw new Exception(string.Format("Multiple {0} label !", label));
            
            return matrix.Values.Column(labelIndexes.First());
        }
    }

}