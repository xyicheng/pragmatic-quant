using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.MonteCarlo.Product
{
    public interface IProductPathFlow
    {
         void ComputePathFlows(ref PathFlows<double, PaymentInfo> pathFlows, 
                               PathFlows<double[], IFixing[]> fixingsPath,
                               PathFlows<double[], PaymentInfo[]> flowRebasementPath);

        PathFlows<double, PaymentInfo> NewPathFlow();
        int SizeOfPath { get; }

        IFixing[] Fixings { get; }
        PaymentInfo[] Payments { get; }
    }
}