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

        public int LocateLeftIndex(double x)
        {
            if (x < abcissae[0]) return -1;
            if (x >= abcissae[abcissae.Length - 1]) return abcissae.Length - 1;

            int jl = 0;
            int ju = abcissae.Length - 1;
            while (ju - jl > 1)
            {
                int jm = (ju + jl) >> 1;
                if (x >= abcissae[jm])
                {
                    jl = jm;
                }
                else
                {
                    ju = jm;
                }
            }
            return jl;
        }
    }
}