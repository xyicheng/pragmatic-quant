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
        public static bool HasCol<TRow, TVal>(LabelledMatrix<TRow, string, TVal> matrix, string label, out int colIndex)
        {
            var labelIndexes = matrix.ColLabels
                                .Select((l, i) => new { Label = l, Index = i })
                                .Where(data => data.Label.Equals(label, StringComparison.InvariantCultureIgnoreCase))
                                .Select(data => data.Index)
                                .ToArray();

            if (!labelIndexes.Any())
            {
                colIndex = -1;
                return false;
            }

            if (labelIndexes.Count() != 1)
                throw new Exception(string.Format("Multiple {0} label !", label));

            colIndex = labelIndexes.First();
            return true;
        }
        public static bool HasCol<TRow, TVal>(this LabelledMatrix<TRow, string, TVal> matrix, string label)
        {
            int index;
            return HasCol(matrix, label, out index);
        }
        public static TVal[] GetColFromLabel<TRow, TVal>(this LabelledMatrix<TRow, string, TVal> matrix, string label)
        {
            int labelIndex;
            if(!HasCol(matrix, label, out labelIndex))
                throw new Exception(string.Format("Missing Label : {0}", label));

            return matrix.Values.Column(labelIndex);
        }
        public static TValOut[] GetColFromLabel<TRow, TVal, TValOut>(this LabelledMatrix<TRow, string, TVal> matrix, string label, Func<TVal,TValOut> func)
        {
            return matrix.GetColFromLabel(label).Map(func);
        }
    }

}