using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.MonteCarlo.Product
{
    public interface IProductPathFlow
    {
        PathFlows<double, PaymentInfo> Compute(PathFlows<double[], IFixing[]> fixingsPath,
                                          PathFlows<double[], PaymentInfo[]> flowRebasementPath);
        IFixing[] Fixings { get; }
        PaymentInfo[] Payments { get; }
        int SizeOfPathInBits { get; }
    }
}