using System.Linq;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.MonteCarlo.Product
{
    public class PriceFlowsAggregator 
        : IPathResultAgregator<PathFlows<double, PaymentInfo>, PathFlows<double, PaymentInfo>>
    {
        public static readonly PriceFlowsAggregator Value = new PriceFlowsAggregator();
        public PathFlows<double, PaymentInfo> Aggregate(PathFlows<double, PaymentInfo>[] paths)
        {
            var prices = new double[paths.First().Flows.Length];
            foreach (PathFlows<double, PaymentInfo> pathFlows in paths)
            {
                VectorUtils.Add(ref prices, pathFlows.Flows);
            }
            VectorUtils.Mult(ref prices, 1.0 / paths.Length);

            return new PathFlows<double, PaymentInfo>(prices, paths.First().Labels);
        }
    }
}