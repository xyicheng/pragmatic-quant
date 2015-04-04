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
}