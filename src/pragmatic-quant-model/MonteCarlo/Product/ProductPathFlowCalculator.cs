using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths.Stochastic;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_model.MonteCarlo.Product
{
    public class ProductPathFlowCalculator 
        : IPathFlowCalculator<double, PaymentInfo>
    {
        #region private fields
        private readonly IPathFlowCalculator<double[], IFixing[]> fixingPathCalculator;
        private readonly IPathFlowCalculator<double[], PaymentInfo[]> numerairePathCalc;
        private readonly IProductPathFlow productPathFlow;
        #endregion
        #region private fields (buffers)
        //TODO Be carefull to clone this field in multithreaded environment
        private PathFlows<double[], IFixing[]> fixingsPath;
        private PathFlows<double[], PaymentInfo[]> numerairePath;
        #endregion

        public ProductPathFlowCalculator(IPathFlowCalculator<double[], IFixing[]> fixingPathCalculator,
                                         IPathFlowCalculator<double[], PaymentInfo[]> numerairePathCalc,
                                         IProductPathFlow productPathFlow)
        {
            this.fixingPathCalculator = fixingPathCalculator;
            this.numerairePathCalc = numerairePathCalc;
            this.productPathFlow = productPathFlow;
            
            //Buffers initialization
            fixingsPath = fixingPathCalculator.NewPathFlow();
            numerairePath = numerairePathCalc.NewPathFlow();
        }

        public void ComputeFlows(ref PathFlows<double, PaymentInfo> pathFlows, IProcessPath processPath)
        {
            fixingPathCalculator.ComputeFlows(ref fixingsPath, processPath);
            numerairePathCalc.ComputeFlows(ref numerairePath, processPath);
            productPathFlow.ComputePathFlows(ref pathFlows, fixingsPath, numerairePath);
        }
        public PathFlows<double, PaymentInfo> NewPathFlow()
        {
            return productPathFlow.NewPathFlow();
        }
        public int SizeOfPath
        {
            get { return productPathFlow.SizeOfPath; }
        }
    }

}