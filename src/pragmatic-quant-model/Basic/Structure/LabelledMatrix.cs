using System;
using System.Collections.Generic;
using System.Linq;

namespace pragmatic_quant_model.Basic.Structure
{
    public class LabelledMatrix<TRow, TCol, TVal>
    {
        #region private fields
        private readonly IDictionary<TCol, int> labelsIndexes;
        #endregion
        public LabelledMatrix(TRow[] rowLabels, TCol[] colLabels, TVal[,] values)
        {
            if (values.GetLength(0) != rowLabels.Length || values.GetLength(1) != colLabels.Length)
                throw new Exception("TimeMatrixData : invalid format datas");

            Values = values;
            ColLabels = colLabels;
            RowLabels = rowLabels;
            labelsIndexes = colLabels.Select((l, i) => new {Lab = l, Index = i})
                .ToDictionary(p => p.Lab, p => p.Index);
        }
        public TRow[] RowLabels { get; private set; }
        public TCol[] ColLabels { get; private set; }
        public TVal[,] Values { get; private set; }

        public bool TryGetCol(TCol label, out TVal[] labelValues)
        {
            int labelIndex;
            if (!labelsIndexes.TryGetValue(label, out labelIndex))
            {
                labelValues = new TVal[0];
                return false;
            }

            labelValues = Values.Column(labelIndex);
            return true;
        }
        public TVal[] GetCol(TCol label)
        {
            TVal[] col;
            if (!TryGetCol(label, out col))
                throw new Exception(String.Format("Missing Label : {0}", label));
            return col;
        }
    }
}