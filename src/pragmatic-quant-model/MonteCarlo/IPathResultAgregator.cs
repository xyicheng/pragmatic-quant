namespace pragmatic_quant_model.MonteCarlo
{
    public interface IPathResultAgregator<in TPath, out TResult>
    {
        TResult Aggregate(TPath[] paths);
    }
    
}