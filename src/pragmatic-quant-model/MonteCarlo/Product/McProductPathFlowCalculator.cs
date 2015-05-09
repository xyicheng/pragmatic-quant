using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.MonteCarlo.Product
{
    public class McProductPathFlowCalculator 
        : IMcPathFlowCalculator<PathFlows<double, PaymentInfo>, PaymentInfo[]>
    {
        #region private fields
        private readonly IMcPathFlowCalculator<PathFlows<double[], IFixing[]>, IFixing[][]> fixingPathCalculator;
        private readonly IMcPathFlowCalculator<PathFlows<double[], PaymentInfo[]>, PaymentInfo[][]> numerairePathCalc;
        private readonly IProductPathFlow productPathFlowInstrument;
        #endregion
        public McProductPathFlowCalculator(IMcPathFlowCalculator<PathFlows<double[], IFixing[]>, IFixing[][]> fixingPathCalculator,
                                           IMcPathFlowCalculator<PathFlows<double[], PaymentInfo[]>, PaymentInfo[][]> numerairePathCalc,
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
        public PaymentInfo[] Labels { get { return productPathFlowInstrument.Payments; } }
        public int SizeOfPathInBits
        {
            get { return productPathFlowInstrument.SizeOfPathInBits; }
        }
    }
}