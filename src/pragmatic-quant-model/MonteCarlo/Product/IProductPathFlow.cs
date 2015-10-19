using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_model.MonteCarlo.Product
{
    public interface IProductPathFlow
    {
        void ComputePathFlows(ref PathFlows<double, CouponFlowLabel> pathFlows, 
                              PathFlows<double[], IFixing[]> fixingsPath,
                              PathFlows<double[], PaymentInfo[]> flowRebasementPath);

        PathFlows<double, CouponFlowLabel> NewPathFlow();
        int SizeOfPath { get; }

        IFixing[] Fixings { get; }
        PaymentInfo[] Payments { get; }
    }
}