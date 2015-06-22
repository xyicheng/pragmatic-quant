using System;

namespace pragmatic_quant_model.Basic
{
    public class StepSearcher
    {
        #region private fields
        private readonly double[] pillars;
        #endregion

        public StepSearcher(double[] pillars)
        {
            if (pillars.Length==0)
                throw new Exception("StepSearcher : pillars array should contain at least one element");

            if (!EnumerableUtils.IsSorted(pillars))
                throw new Exception("StepSearcher : pillars array is assumed to be sorted");

            this.pillars = pillars;
        }

        public double[] Pillars { get { return pillars; } }

        /// <summary>
        /// Locate index i such that x is in right open interval :
        ///     [ abscissae[i], abscissae[i+1] [ 
        /// </summary>
        public int LocateLeftIndex(double x)
        {
            if (x < pillars[0]) return -1;
            if (x >= pillars[pillars.Length - 1]) return pillars.Length - 1;

            int jl = 0;
            int ju = pillars.Length - 1;
            while (ju - jl > 1)
            {
                int jm = (ju + jl) >> 1;
                if (x < pillars[jm])
                {
                    ju = jm;
                }
                else
                {
                    jl = jm;
                }
            }
            return jl;
        }

        /// <summary>
        /// Find index i such that x is equal to abscissae[i]
        /// </summary>
        public bool TryFindPillarIndex(double x, out int i, double precision)
        {
            var leftIndex = LocateLeftIndex(x);

            if ((leftIndex >= 0)
                && DoubleUtils.Equality(x, pillars[leftIndex], precision))
            {
                i = leftIndex;
                return true;
            }

            if ((leftIndex + 1 < pillars.Length)
                && DoubleUtils.Equality(x, pillars[leftIndex + 1], precision))
            {
                i = leftIndex + 1;
                return true;
            }

            i = -1;
            return false;
        }

        /// <summary>
        /// Find index i such that x is equal to abscissae[i]
        /// </summary>
        public bool TryFindPillarIndex(double x, out int i)
        {
            return TryFindPillarIndex(x, out i, DoubleUtils.MachineEpsilon);
        }
    }
}