using System;
using pragmatic_quant_model.Model;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.MonteCarlo
{
    public class McModel
    {
        public IFactorModelRepresentation FactorRepresentation { get; private set; }

        public DateTime[] SimulatedDates { get; private set; }
        public IRandomGenerator RandomGenerator { get; private set; }
        public IProcessPathGenerator ProcessPathGen { get; private set; }
        public PaymentInfo ProbaMeasure { get; private set; }
        public double Numeraire0 { get; private set; }
    }

    public abstract class McModelFactory<TModel> where TModel : IModel
    {
        public abstract McModel Build(TModel model, DateTime[] simulatedDates);
    }
}