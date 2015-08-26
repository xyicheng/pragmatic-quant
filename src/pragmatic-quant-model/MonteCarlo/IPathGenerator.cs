namespace pragmatic_quant_model.MonteCarlo
{
    public interface IPathGenerator<TPath>
    {
        void ComputePath(ref TPath path, double[] randoms);

        TPath NewPath();
        int SizeOfPath { get; }
    }
}