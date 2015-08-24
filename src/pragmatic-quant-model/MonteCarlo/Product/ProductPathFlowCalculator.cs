using pragmatic_quant_model.Maths.Stochastic;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.MonteCarlo.Product
{
    public class ProductPathFlowCalculator 
        : IPathFlowCalculator<double, PaymentInfo>
    {
        #region private fields
        private readonly IPathFlowCalculator<double[], IFixing[]> fixingPathCalculator;
        private readonly IPathFlowCalculator<double[], PaymentInfo[]> numerairePathCalc;
        private readonly IProductPathFlow productPathFlowInstrument;
        #endregion
        public ProductPathFlowCalculator(IPathFlowCalculator<double[], IFixing[]> fixingPathCalculator,
                                         IPathFlowCalculator<double[], PaymentInfo[]> numerairePathCalc,
                                         IProductPathFlow productPathFlowInstrument)
        {
            this.fixingPathCalculator = fixingPathCalculator;
            this.numerairePathCalc = numerairePathCalc;
            this.productPathFlowInstrument = productPathFlowInstrument;
        }

        public PathFlows<double, PaymentInfo> Compute(IProcessPath processPath)
        {
            var fixingsPath = fixingPathCalculator.Compute(processPath);
            var numerairePath = numerairePathCalc.Compute(processPath);
            return productPathFlowInstrument.Compute(fixingsPath, numerairePath);
        }
        public int SizeOfPathInBits
        {
            get { return productPathFlowInstrument.SizeOfPathInBits; }
        }
    }
}