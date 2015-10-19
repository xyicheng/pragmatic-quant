using System.Linq;
using pragmatic_quant_model.Maths;

namespace pragmatic_quant_model.MonteCarlo.Product
{
    public class PriceFlowsAggregator<TLabel>
        : IPathResultAgregator<PathFlows<double, TLabel>, PathFlows<double, TLabel>>
    {
        public PathFlows<double, TLabel> Aggregate(PathFlows<double, TLabel>[] paths)
        {
            var prices = new double[paths.First().Flows.Length];
            foreach (PathFlows<double, TLabel> pathFlows in paths)
            {
                VectorUtils.Add(ref prices, pathFlows.Flows);
            }
            VectorUtils.Mult(ref prices, 1.0 / paths.Length);

            return new PathFlows<double, TLabel>(prices, paths.First().Labels);
        }
    }
}