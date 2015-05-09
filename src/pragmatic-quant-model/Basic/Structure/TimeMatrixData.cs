using System.Diagnostics.Contracts;
using pragmatic_quant_model.Basic.Dates;

namespace pragmatic_quant_model.Basic.Structure
{
    public class TimeMatrixDatas : LabelledMatrix<DateOrDuration, string, double>
    {
        public TimeMatrixDatas(DateOrDuration[] rowLabels, string[] colLabels, double[,] values)
            : base(rowLabels, colLabels, values)
        {
        }
        public TimeMatrixDatas(LabelledMatrix<DateOrDuration, string, double> timeMatrixData)
            : this(timeMatrixData.RowLabels, timeMatrixData.ColLabels, timeMatrixData.Values)
        {
        }
    }


    public class MapRawDatas<TP, TV>
    {
        public MapRawDatas(TP[] pillars, TV[] values)
        {
            Contract.Requires(pillars.Length == values.Length);
            Values = values;
            Pillars = pillars;
        }
        public TP[] Pillars { get; private set; }
        public TV[] Values { get; private set; }
    }
    
}