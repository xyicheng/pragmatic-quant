namespace pragmatic_quant_model.MonteCarlo
{
    public class PathFlows<TFlow, TLabel>
    {
        public PathFlows(TFlow[] flows, TLabel[] labels)
        {
            Labels = labels;
            Flows = flows;
        }
        public TFlow[] Flows { get; private set; }
        public TLabel[] Labels { get; private set; }
    }
}