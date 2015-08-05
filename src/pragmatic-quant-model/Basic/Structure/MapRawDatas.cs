using System.Diagnostics.Contracts;

namespace pragmatic_quant_model.Basic.Structure
{
    public class MapRawDatas<TP, TV>
    {
        public MapRawDatas(TP[] pillars, TV[] values)
        {
            Contract.Requires(pillars.Length == values.Length);
            Values = values;
            Pillars = pillars;
        }
        public TP[] Pillars { get; private set; }
        public TV[] Values { get; private set; }
    }
    
}