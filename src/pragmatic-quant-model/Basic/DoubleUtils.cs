
using System;

namespace pragmatic_quant_model.Basic
{
    public static class DoubleUtils
    {
        #region private methods
        private static double GetMachineEpsilon()
        {
            double machEps = 1.0d;
            do
            {
                machEps /= 2.0d;
            } // ReSharper disable CompareOfFloatsByEqualityOperator
            while ((1.0 + machEps) != 1.0);
            // ReSharper restore CompareOfFloatsByEqualityOperator
            return machEps;
        }
        #endregion
        public static readonly double Epsilon = GetMachineEpsilon();
        public static bool MachineEquality(double x, double y)
        {
            return Math.Abs(x - y) <= Math.Max(Math.Abs(x), Math.Abs(y)) * Epsilon;
        }

        public static bool EqualZero(double x)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return x == 0.0;
        }
    }
}
