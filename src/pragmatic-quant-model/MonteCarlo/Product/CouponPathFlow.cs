using System;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_model.MonteCarlo.Product
{
    public class CouponPathFlow
    {
        public CouponPathFlow(FixingFuncPathValue payoffPathValues, 
                              CouponFlowLabel couponFlowLabel, 
                              Tuple<int, int> paymentCoordinate)
        {
            PayoffPathValues = payoffPathValues;
            CouponLabel = couponFlowLabel;
            PaymentCoordinate = paymentCoordinate;
        }
        
        public FixingFuncPathValue PayoffPathValues { get; private set; }
        public Tuple<int, int> PaymentCoordinate { get; private set; }
        public CouponFlowLabel CouponLabel { get; private set; }
        public double FlowValue(double[][] simulatedFixings, double[][] rebasements)
        {
            double flowRebasement = rebasements[PaymentCoordinate.Item1][PaymentCoordinate.Item2];
            return PayoffPathValues.Value(simulatedFixings) * flowRebasement;
        }

        public CouponPathFlow Copy()
        {
            return new CouponPathFlow(PayoffPathValues.Copy(), CouponLabel, PaymentCoordinate);
        }
    }
    public class FixingFuncPathValue
    {
        #region private fields (buffer)
        private readonly double[] fixingBuffer;
        #endregion

        public FixingFuncPathValue(IFixingFunction fixingFunction,
                                        Tuple<int, int>[] fixingCoordinates)
        {
            FixingFunction = fixingFunction;
            FixingCoordinates = fixingCoordinates;

            //Buffer initialization
            fixingBuffer = new double[fixingFunction.Fixings.Length];
        }

        public Tuple<int, int>[] FixingCoordinates { get; private set; }
        public IFixingFunction FixingFunction { get; private set; }
        public double Value(double[][] simulatedFixings)
        {
            for (int i = 0; i < FixingCoordinates.Length; i++)
            {
                Tuple<int, int> coordinate = FixingCoordinates[i];
                fixingBuffer[i] = simulatedFixings[coordinate.Item1][coordinate.Item2];
            }
            return FixingFunction.Value(fixingBuffer);
        }

        public FixingFuncPathValue Copy()
        {
            return new FixingFuncPathValue(FixingFunction, FixingCoordinates);
        }
    }
}