using System;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.Model
{
    public interface IFactorModelRepresentation
    {
        Func<double[], double> this[IObservation observation] { get; }
    }
}