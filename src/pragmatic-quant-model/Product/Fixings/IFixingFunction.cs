using System;
using System.Linq;

namespace pragmatic_quant_model.Product.Fixings
{
    public interface IFixingFunction : IObservation
    {
        IFixing[] Fixings { get; }
        double Value(double[] fixings);
    }

    public class GenericFixingFunction : IFixingFunction
    {
        #region private fields
        private readonly Func<double[], double> value;
        #endregion
        public GenericFixingFunction(IFixing[] fixings, Func<double[], double> value)
        {
            Fixings = fixings;
            this.value = value;
        }
        public DateTime Date
        {
            get
            {
                return Fixings.Length > 0
                     ? Fixings.Select(f => f.Date).Max()
                     : DateTime.MinValue;
            }
        }
        public IFixing[] Fixings { get; private set; }
        public double Value(double[] fixings)
        {
            return value(fixings);
        }
    }

    public class WeightedFixingFunction : IFixingFunction
    {
        #region private fields
        private readonly double weigth;
        private readonly IFixingFunction fixingFunc;
        #endregion
        public WeightedFixingFunction(double weigth, IFixingFunction fixingFunc)
        {
            this.weigth = weigth;
            this.fixingFunc = fixingFunc;
        }
        public DateTime Date
        {
            get { return fixingFunc.Date; }
        }
        public IFixing[] Fixings
        {
            get { return fixingFunc.Fixings; }
        }
        public double Value(double[] fixings)
        {
            return weigth * fixingFunc.Value(fixings);
        }
    }
}