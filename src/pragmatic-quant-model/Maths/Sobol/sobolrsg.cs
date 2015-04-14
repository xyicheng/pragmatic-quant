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

using System;

namespace pragmatic_quant_model.Maths.Sobol {
    
    /*! A Gray code counter and bitwise operations are used for very
        fast sequence generation.

        The implementation relies on primitive polynomials modulo two
        from the book "Monte Carlo Methods in Finance" by Peter
        Jдckel.

        21 200 primitive polynomials modulo two are provided in QuantLib.
        Jдckel has calculated 8 129 334 polynomials: if you need that many
        dimensions you can replace the primitivepolynomials.c file included
        in QuantLib with the one provided in the CD of the "Monte Carlo
        Methods in Finance" book.

        The choice of initialization numbers (also know as free direction
        integers) is crucial for the homogeneity properties of the sequence.
        Sobol defines two homogeneity properties: Property A and Property A'.

        The unit initialization numbers suggested in "Numerical
        Recipes in C", 2nd edition, by Press, Teukolsky, Vetterling,
        and Flannery (section 7.7) fail the test for Property A even
        for low dimensions.

        Bratley and Fox published coefficients of the free direction
        integers up to dimension 40, crediting unpublished work of
        Sobol' and Levitan. See Bratley, P., Fox, B.L. (1988)
        "Algorithm 659: Implementing Sobol's quasirandom sequence
        generator," ACM Transactions on Mathematical Software
        14:88-100. These values satisfy Property A for d<=20 and d =
        23, 31, 33, 34, 37; Property A' holds for d<=6.

        Jдckel provides in his book (section 8.3) initialization
        numbers up to dimension 32. Coefficients for d<=8 are the same
        as in Bradley-Fox, so Property A' holds for d<=6 but Property
        A holds for d<=32.

        The implementation of Lemieux, Cieslak, and Luttmer includes
        coefficients of the free direction integers up to dimension
        360.  Coefficients for d<=40 are the same as in Bradley-Fox.
        For dimension 40<d<=360 the coefficients have
        been calculated as optimal values based on the "resolution"
        criterion. See "RandQMC user's guide - A package for
        randomized quasi-Monte Carlo methods in C," by C. Lemieux,
        M. Cieslak, and K. Luttmer, version January 13 2004, and
        references cited there
        (http://www.math.ucalgary.ca/~lemieux/randqmc.html).
        The values up to d<=360 has been provided to the QuantLib team by
        Christiane Lemieux, private communication, September 2004.

        For more info on Sobol' sequences see also "Monte Carlo
        Methods in Financial Engineering," by P. Glasserman, 2004,
        Springer, section 5.2.3

        The Joe--Kuo numbers and the Kuo numbers are due to Stephen Joe
        and Frances Kuo.

        S. Joe and F. Y. Kuo, Constructing Sobol sequences with better
        two-dimensional projections, preprint Nov 22 2007

        See http://web.maths.unsw.edu.au/~fkuo/sobol/ for more information.

        Note that the Kuo numbers were generated to work with a
        different ordering of primitive polynomials for the first 40
        or so dimensions which is why we have the Alternative
        Primitive Polynomials.

        \test
        - the correctness of the returned values is tested by
          reproducing known good values.
        - the correctness of the returned values is tested by checking
          their discrepancy against known good values.
    */
    /// <summary>
    /// ! Sobol low-discrepancy sequence generator
    /// </summary>
    public partial class SobolRsg
    {
        #region consts fields
        const int Bits = 8 * sizeof(uint);
        const double Ln2 = 0.693147180559945309417;
        // 1/(2^bits_) (written as (1/2)/(2^(bits_-1)) to avoid long overflow)
        const double NormalizationFactor = 0.5 / (1UL << (Bits - 1));
        #endregion
        #region private fields
        private readonly int dimensionality;
        private readonly double[] sequence;
        private readonly uint[] integerSequence;
        private readonly uint[][] directionIntegers;
        #endregion
        #region private mutable fields
        private ulong sequenceCounter;
        private bool firstDraw;
        #endregion
        #region private methods (initialisation)
        private static void FillDirection(uint[][] directionIntegers, int maxDim, uint[][] initializer)
        {
            if (maxDim > initializer.Length + 1)
                throw new ApplicationException("dimensionality " + maxDim + " exceeds the number of available ");

            for (int k = 1; k < maxDim; k++)
            {
                int j = 0;
                // 0 marks coefficients' end for a given dimension
                while (initializer[k - 1][j] != 0)
                {
                    directionIntegers[j][k] = initializer[k - 1][j];
                    directionIntegers[j][k] <<= (Bits - j - 1);
                    j++;
                }
            }
        }
        private static void FillDegreeAndPolynom(int dimensionality, SobolDirection direction,
            out uint[] degree, out long[] ppmt)
        {
            // initializes coefficient array of the k-th primitive polynomial
            // and degree of the k-th primitive polynomial
            degree = new uint[dimensionality];
            ppmt = new long[dimensionality];

            bool useAltPolynomials = direction == SobolDirection.Kuo
                                     || direction == SobolDirection.Kuo2
                                     || direction == SobolDirection.Kuo3
                                     || direction == SobolDirection.SobolLevitan
                                     || direction == SobolDirection.SobolLevitanLemieux;

            // degree 0 is not used
            ppmt[0] = 0;
            degree[0] = 0;
            uint currentDegree = 1;
            uint altDegree = useAltPolynomials ? MaxAltDegree : 0;
            int k = 1;
            int index = 0;
            for (; k < Math.Min(dimensionality, altDegree); k++, index++)
            {
                ppmt[k] = AltPrimitivePolynomials[currentDegree - 1][index];
                if (ppmt[k] == -1)
                {
                    ++currentDegree;
                    index = 0;
                    ppmt[k] = AltPrimitivePolynomials[currentDegree - 1][index];
                }

                degree[k] = currentDegree;
            }

            for (; k < dimensionality; k++, index++)
            {
                ppmt[k] = PrimitivePolynomials[currentDegree - 1][index];
                if (ppmt[k] == -1)
                {
                    ++currentDegree;
                    index = 0;
                    ppmt[k] = PrimitivePolynomials[currentDegree - 1][index];
                }
                degree[k] = currentDegree;

            }
        }
        private void DirectionInit(SobolDirection direction)
        {
            // initializes bits_ direction integers for each dimension
            // and store them into directionIntegers_[dimensionality_][bits_]
            //
            // In each dimension k with its associated primitive polynomial,
            // the first degree_[k] direction integers can be chosen freely
            // provided that only the l leftmost bits can be non-zero, and
            // that the l-th leftmost bit must be set
            int j;
            for (j = 0; j < Bits; j++)
                directionIntegers[j][0] = (uint)(1 << (Bits - j - 1));

            switch (direction)
            {
                case SobolDirection.Jaeckel:
                    FillDirection(directionIntegers, dimensionality, JaeckelInitializers.Value);
                    break;

                case SobolDirection.SobolLevitan:
                    FillDirection(directionIntegers, dimensionality, SLinitializers.Value);
                    break;

                case SobolDirection.SobolLevitanLemieux:
                    FillDirection(directionIntegers, dimensionality, Linitializers.Value);
                    break;

                case SobolDirection.JoeKuoD5:
                    FillDirection(directionIntegers, dimensionality, JoeKuoD5Initializers.Value);
                    break;

                case SobolDirection.JoeKuoD6:
                    FillDirection(directionIntegers, dimensionality, JoeKuoD6Initializers.Value);
                    break;

                case SobolDirection.JoeKuoD7:
                    FillDirection(directionIntegers, dimensionality, JoeKuoD7Initializers.Value);
                    break;

                case SobolDirection.Kuo:
                    FillDirection(directionIntegers, dimensionality, KuoInitializers.Value);
                    break;

                case SobolDirection.Kuo2:
                    FillDirection(directionIntegers, dimensionality, Kuo2Initializers.Value);
                    break;

                case SobolDirection.Kuo3:
                    FillDirection(directionIntegers, dimensionality, Kuo3Initializers.Value);
                    break;
            }
        }
        #endregion 
        
        public SobolRsg(int dimensionality) 
            : this(dimensionality, SobolDirection.Jaeckel) { }
        public SobolRsg(int dimensionality, SobolDirection direction)
        {
            if (dimensionality < 0) throw new ApplicationException("dimensionality must be greater than 0");
            if (!(dimensionality <= PPMT_MAX_DIM))
                throw new ApplicationException("dimensionality " + dimensionality + " exceeds the number of available "
                                               + "primitive polynomials modulo two (" + PPMT_MAX_DIM + ")");

            this.dimensionality = dimensionality;
            sequenceCounter = 0;
            firstDraw = true;
            integerSequence = new uint[dimensionality];
            
            sequence = new double[dimensionality];
            for (int i = 0; i < dimensionality; ++i) sequence[i] = 1.0;
            
            directionIntegers = new uint[Bits][];
            for (int i = 0; i < Bits; i++)
                directionIntegers[i] = new uint[dimensionality];

            uint[] degree;
            long[] ppmt;
            FillDegreeAndPolynom(this.dimensionality, direction, out degree, out ppmt);

            DirectionInit(direction);

            // computation of directionIntegers_[k][l] for l>=degree_[k]
            // by recurrence relation
            for (int k = 1; k < this.dimensionality; k++)
            {
                uint gk = degree[k];
                for (var l = (int) gk; l < Bits; l++)
                {
                    // eq. 8.19 "Monte Carlo Methods in Finance" by P. Jдckel
                    uint n = (directionIntegers[(int) (l - gk)][k] >> (int) gk);
                    // a[k][j] are the coefficients of the monomials in ppmt[k]
                    // The highest order coefficient a[k][0] is not actually
                    // used in the recurrence relation, and the lowest order
                    // coefficient a[k][gk] is always set: this is the reason
                    // why the highest and lowest coefficient of
                    // the polynomial ppmt[k] are not included in its encoding,
                    // provided that its degree is known.
                    // That is: a[k][j] = ppmt[k] >> (gk-j-1)
                    for (uint z = 1; z < gk; z++)
                    {
                        // XORed with a selection of (unshifted) direction
                        // integers controlled by which of the a[k][j] are set
                        if ((((ulong) ppmt[k] >> (int) (gk - z - 1)) & 1UL) != 0)
                            n ^= directionIntegers[(int) (l - z)][k];
                    }
                    // a[k][gk] is always set, so directionIntegers_[k][l-gk]
                    // will always enter
                    n ^= directionIntegers[(int) (l - gk)][k];
                    directionIntegers[l][k] = n;
                }
            }

            // initialize the Sobol integer/double vectors
            // first draw
            for (int k = 0; k < this.dimensionality; k++)
            {
                integerSequence[k] = directionIntegers[0][k];
            }
        }

        /// <summary>
        /// Skip to the n-th sample in the low-discrepancy sequence
        /// </summary>
        /// <param name="skip"></param>
        public void SkipTo(ulong skip)
        {
            ulong n = skip + 1;
            uint ops = (uint) (Math.Log(n) / Ln2) + 1;

            // Convert to Gray code
            ulong g = n ^ (n >> 1);
            for (int k = 0; k < dimensionality; k++)
            {
                integerSequence[k] = 0;
                for (int index = 0; index < ops; index++)
                {
                    if ((g >> index & 1) != 0)
                        integerSequence[k] ^= directionIntegers[index][k];
                }
            }
            sequenceCounter = skip;
        }
        public uint[] NextInt32Sequence()
        {
            if (firstDraw)
            {
                // it was precomputed in the constructor
                firstDraw = false;
                return integerSequence;
            }
            // increment the counter
            sequenceCounter++;
            // did we overflow?
            if (sequenceCounter == 0) throw new ApplicationException("period exceeded");

            // instead of using the counter n as new unique generating integer
            // for the n-th draw use the Gray code G(n) as proposed
            // by Antonov and Saleev
            ulong n = sequenceCounter;
            // Find rightmost zero bit of n
            int j = 0;
            while ((n & 1) != 0)
            {
                n >>= 1;
                j++;
            }
            var j_direction = directionIntegers[j];
            for (int k = 0; k < dimensionality; k++)
            {
                // XOR the appropriate direction number into each component of
                // the integer sequence to obtain a new Sobol integer for that
                // component
                integerSequence[k] ^= j_direction[k];
            }
            return integerSequence;
        }
        public double[] NextSequence()
        {
            var v = NextInt32Sequence();
            for (int k = 0; k < dimensionality; ++k)
                sequence[k] = v[k] * NormalizationFactor; // normalize to get a double in (0,1)
            return sequence;
        }
        public double[] LastSequence() { return sequence; }
        public int Dimension() { return dimensionality; }
    }

    public enum SobolDirection
    {
        Jaeckel,
        SobolLevitan,
        SobolLevitanLemieux,
        JoeKuoD5,
        JoeKuoD6,
        JoeKuoD7,
        Kuo,
        Kuo2,
        Kuo3
    };
}
