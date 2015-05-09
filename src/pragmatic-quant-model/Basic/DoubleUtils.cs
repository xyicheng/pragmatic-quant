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
        public static readonly double MachineEpsilon = GetMachineEpsilon();
        
        //Todo : move this away
        public static readonly double Sqrt_Epsilon = Math.Sqrt(MachineEpsilon);
        public static readonly double FourthRoot_Epsilon = Math.Sqrt(Sqrt_Epsilon);
        public static readonly double EighthRoot_Epsilon = Math.Sqrt(FourthRoot_Epsilon);
        public static readonly double SixteenthRoot_Epsilon = Math.Sqrt(EighthRoot_Epsilon);
        
        public static bool MachineEquality(double x, double y)
        {
            return Equality(x, y, MachineEpsilon);
        }
        public static bool Equality(double x, double y, double relativePrecision)
        {
            if (double.IsInfinity(x) || double.IsInfinity(y))
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                return x == y;
            }
            
            return Math.Abs(x - y) <= Math.Max(Math.Abs(x), Math.Abs(y)) * relativePrecision;
        }
        public static bool EqualZero(double x)
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            return x == 0.0;
        }
    }
}
