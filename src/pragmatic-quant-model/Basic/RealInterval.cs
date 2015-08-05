using System.Diagnostics.Contracts;

namespace pragmatic_quant_model.Basic
{
    public class RealInterval
    {
        public RealInterval(double inf, double sup, bool isLeftOpen, bool isRightOpen)
        {
            Contract.Requires(inf <= sup);
            Inf = inf;
            Sup = sup;
            IsLeftOpen = isLeftOpen;
            IsRightOpen = isRightOpen;
        }

        public static RealInterval Compact(double inf, double sup)
        {
            return new RealInterval(inf, sup, false, false);
        }
        public static RealInterval RightOpen(double inf, double sup)
        {
            return new RealInterval(inf, sup, false, true);
        }
        public static RealInterval LeftOpen(double inf, double sup)
        {
            return new RealInterval(inf, sup, true, false);
        }

        public readonly double Inf;
        public readonly double Sup;
        public readonly bool IsLeftOpen;
        public readonly bool IsRightOpen;
        public double Length
        {
            get { return Sup - Inf; }
        }

        public bool Contain(double x)
        {
            return (IsLeftOpen ? x > Inf : x >= Inf)
                   && (IsRightOpen ? x < Sup : x <= Sup);
        }
        public override string ToString()
        {
            //TODO treat special case : empty and singleton
            return (IsLeftOpen ? "]" : "[")
                   + Inf + "," + Sup
                   + (IsRightOpen ? "[" : "]");
        }
    }



}
