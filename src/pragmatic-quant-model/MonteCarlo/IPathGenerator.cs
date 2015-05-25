namespace pragmatic_quant_model.MonteCarlo
{
    public interface IPathGenerator<out TPath>
    {
        TPath ComputePath(double[] randoms);
        int SizeOfPathInBits { get; }
    }
}