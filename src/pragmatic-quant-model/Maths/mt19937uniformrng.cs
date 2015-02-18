/*
 Copyright (C) 2008 Siarhei Novik (snovik@gmail.com)
  
 This file is part of QLNet Project http://qlnet.sourceforge.net/

 QLNet is free software: you can redistribute it and/or modify it
 under the terms of the QLNet license.  You should have received a
 copy of the license along with this program; if not, license is  
 available online at <http://qlnet.sourceforge.net/License.html>.
  
 QLNet is a based on QuantLib, a free-software/open-source library
 for financial quantitative analysts and developers - http://quantlib.org/
 The QuantLib license is available online at http://quantlib.org/license.shtml.
 
 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System.Collections.Generic;

namespace pragmatic_quant_model.Maths
{
    
    ///<summary>
    ///  Mersenne Twister uniform random number generator of period 2**19937-1
    ///   For more details see http://www.math.keio.ac.jp/matumoto/emt.html
    ///</summary>
    public class MersenneTwisterUniformRng
    {
        #region const
        // Period parameters
        const int N = 624;
        const int M = 397;
        // constant vector a
        const ulong Matrix_A = 0x9908b0dfUL;
        // most significant w-r bits
        const ulong Upper_Mask = 0x80000000UL;
        // least significant r bits
        const ulong Lower_Mask = 0x7fffffffUL;
        #endregion
        #region private fields
        private List<ulong> mt;
        private int mti;
        #endregion
        #region private methods
        private void SeedInitialization(ulong seed)
        {
            /* initializes mt with a seed */
            ulong s = seed;
            mt[0] = s & 0xffffffffUL;
            for (mti = 1; mti < N; mti++)
            {
                mt[mti] = (1812433253UL * (mt[mti - 1] ^ (mt[mti - 1] >> 30)) + (ulong)mti);
                /* See Knuth TAOCP Vol2. 3rd Ed. P.106 for multiplier. */
                /* In the previous versions, MSBs of the seed affect   */
                /* only MSBs of the array mt[].                        */
                /* 2002/01/09 modified by Makoto Matsumoto             */
                mt[mti] &= 0xffffffffUL;
                /* for >32 bit machines */
            }
        }
        #endregion
        public MersenneTwisterUniformRng() : this(0) { }
        public MersenneTwisterUniformRng(ulong seed) {
            mt = new List<ulong>(N);
            SeedInitialization(seed);
        }
        public MersenneTwisterUniformRng(List<ulong> seeds) {
            mt = new List<ulong>(N);

            SeedInitialization(19650218UL);
            int i = 1, j = 0, k = (N > seeds.Count ? N : seeds.Count);
            for (; k!=0; k--) {
                mt[i] = (mt[i] ^ ((mt[i - 1] ^ (mt[i - 1] >> 30)) * 1664525UL)) + seeds[j] + (ulong)j; /* non linear */
                mt[i] &= 0xffffffffUL; /* for WORDSIZE > 32 machines */
                i++; j++;
                if (i>=N) { mt[0] = mt[N-1]; i=1; }
                if (j>=seeds.Count) j=0;
            }
            for (k=N-1; k!=0; k--) {
                mt[i] = (mt[i] ^ ((mt[i-1] ^ (mt[i-1] >> 30)) * 1566083941UL)) - (ulong)i; /* non linear */
                mt[i] &= 0xffffffffUL; /* for WORDSIZE > 32 machines */
                i++;
                if (i>=N) { mt[0] = mt[N-1]; i=1; }
            }

            mt[0] = 0x80000000UL; /*MSB is 1; assuring non-zero initial array*/
        }
        
        /// <summary>
        /// Return a sample with weight 1.0 containing a random number on (0.0, 1.0)-real-interval
        /// </summary>
        public double Next() {
            // divide by 2^32
            double result = ((double)NextInt32() + 0.5)/4294967296.0;
            return result;
        }

        /// <summary>
        /// Return  a random number on [0,0xffffffff]-interval
        /// </summary>
        public ulong NextInt32() {
            ulong y;
            ulong[] mag01 = { 0x0UL, Matrix_A };
            /* mag01[x] = x * MATRIX_A  for x=0,1 */

            if (mti >= N) { /* generate N words at one time */
                int kk;

                for (kk=0;kk<N-M;kk++) {
                    y = (mt[kk]&Upper_Mask)|(mt[kk+1]&Lower_Mask);
                    mt[kk] = mt[kk+M] ^ (y >> 1) ^ mag01[y & 0x1UL];
                }
                for (;kk<N-1;kk++) {
                    y = (mt[kk]&Upper_Mask)|(mt[kk+1]&Lower_Mask);
                    mt[kk] = mt[kk+(M-N)] ^ (y >> 1) ^ mag01[y & 0x1UL];
                }
                y = (mt[N-1]&Upper_Mask)|(mt[0]&Lower_Mask);
                mt[N-1] = mt[M-1] ^ (y >> 1) ^ mag01[y & 0x1UL];

                mti = 0;
            }

            y = mt[mti++];

            /* Tempering */
            y ^= (y >> 11);
            y ^= (y << 7) & 0x9d2c5680UL;
            y ^= (y << 15) & 0xefc60000UL;
            y ^= (y >> 18);

            return y;
        }
    }
}
