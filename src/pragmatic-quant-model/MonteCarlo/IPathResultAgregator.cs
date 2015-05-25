namespace pragmatic_quant_model.MonteCarlo
{
    public interface IPathResultAgregator<in TPathResult, out TResult>
    {
        TResult Aggregate(TPathResult[] paths);
    }
    
}