using System;

namespace pragmatic_quant_model.Basic
{
    public class StepSearcher
    {
        #region private fields
        private readonly double[] abcissae;
        #endregion
        public StepSearcher(double[] abcissae)
        {
            if (abcissae.Length==0)
                throw new Exception("StepSearcher : abcissae array should contain at least one element");

            if (!EnumerableUtils.IsSorted(abcissae))
                throw new Exception("StepSearcher : abcissae array is assumed to be sorted");

            this.abcissae = abcissae;
        }

        /// <summary>
        /// Locate index i such that x is in right open interval :
        ///     [ abscissae[i], abscissae[i+1] [ 
        /// </summary>
        public int LocateLeftIndex(double x)
        {
            if (x < abcissae[0]) return -1;
            if (x >= abcissae[abcissae.Length - 1]) return abcissae.Length - 1;

            int jl = 0;
            int ju = abcissae.Length - 1;
            while (ju - jl > 1)
            {
                int jm = (ju + jl) >> 1;
                if (x < abcissae[jm])
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

            if (DoubleUtils.Equality(x, abcissae[leftIndex], precision))
            {
                i = leftIndex;
                return true;
            }

            if ((leftIndex + 1 < abcissae.Length)
                && DoubleUtils.Equality(x, abcissae[leftIndex + 1], precision))
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