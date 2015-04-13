﻿/*
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
 FOR A PARTICAR PURPOSE.  See the license for more details.
*/


using System;

namespace pragmatic_quant_model.Maths.Sobol
{

#if NOPOLY
   public partial class SobolRsg
   {
      const uint maxAltDegree = 0;
      private readonly static long[][] AltPrimitivePolynomials;
      private static readonly ulong[][] initializers;
      private static readonly ulong[][] SLinitializers;
      private static readonly ulong[][] Linitializers;
      private static readonly ulong[][] JoeKuoD5initializers;
      private static readonly ulong[][] JoeKuoD6initializers;
      private static readonly ulong[][] JoeKuoD7initializers;
      private static readonly ulong[][] Kuoinitializers;
      private static readonly ulong[][] Kuo2initializers;
      private static readonly ulong[][] Kuo3initializers;
   }
#else
   public partial class SobolRsg {
        // number of dimensions in the alternative primitive polynomials
        const uint maxAltDegree = 52;

        private static readonly long[] AltPrimitivePolynomialDegree01 =
        {
            0, /* x+1 (1)(1) */
            -1
        };

        private readonly static long[] AltPrimitivePolynomialDegree02 =
        {
            1, /* x^2+x+1 (1)1(1) */
            -1
        };

        private readonly static long[] AltPrimitivePolynomialDegree03 =
        {
            1, /* x^3    +x+1 (1)01(1) */
            2, /* x^3+x^2  +1 (1)10(1) */
            -1
        };

        private readonly static long[] AltPrimitivePolynomialDegree04 =
        {
            1, /* x^4+       +x+1 (1)001(1) */
            4, /* x^4+x^3+     +1 (1)100(1) */
            -1
        };


        private readonly static long[] AltPrimitivePolynomialDegree05 =
        {
            2,  /* x^5        +x^2  +1 (1)0010(1) */
            13, /* x^5+x^4+x^3    +x+1 (1)1101(1) */
            7,  /* x^5    +x^3+x^2+x+1 (1)0111(1) */
            14, /* x^5+x^4+x^3+x^2  +1 (1)1110(1) */
            11, /* x^5+x^4    +x^2+x+1 (1)1011(1) */
            4,  /* x^5    +x^3      +1 (1)0100(1) */
            -1
        };

        private readonly static long[] AltPrimitivePolynomialDegree06 =
        {
            1,  /* x^6                +x+1 (1)00001(1) */
            16, /* x^6+x^5              +1 (1)10000(1) */
            13, /* x^6    +x^4+x^3    +x+1 (1)01101(1) */
            22, /* x^6+x^5    +x^3+x^2  +1 (1)10110(1) */
            19, /* x^6            +x^2+x+1 (1)10011(1) */
            25, /* x^6+x^5+x^4        +x+1 (1)11001(1) */
            -1
        };


        private readonly static long[] AltPrimitivePolynomialDegree07 =
        {
            1,  /* x^7                    +x+1 (1)000001(1) */
            32, /* x^7+x^6                  +1 (1)100000(1) */
            4,  /* x^7            +x^3      +1 (1)000100(1) */
            8,  /* x^7        +x^4          +1 (1)001000(1) */
            7,  /* x^7            +x^3+x^2+x+1 (1)000111(1) */
            56, /* x^7+x^6+x^5+x^4          +1 (1)111000(1) */
            14, /* x^7        +x^4+x^3+x^2  +1 (1)001110(1) */
            28, /* x^7    +x^5+x^4+x^3      +1 (1)011100(1) */
            19, /* x^7    +x^5        +x^2+x+1 (1)010011(1) */
            50, /* x^7+x^6+x^5        +x^2  +1 (1)110010(1) */
            21, /* x^7    +x^5    +x^3    +x+1 (1)010101(1) */
            42, /* x^7+x^6    +x^4    +x^2  +1 (1)101010(1) */
            31, /* x^7    +x^5+x^4+x^3+x^2+x+1 (1)011111(1) */
            62, /* x^7+x^6+x^5+x^4+x^3+x^2  +1 (1)111110(1) */
            37, /* x^7+x^6        +x^3    +x+1 (1)100101(1) */
            41, /* x^7+x^6    +x^4        +x+1 (1)101001(1) */
            55, /* x^7+x^6+x^5    +x^3+x^2+x+1 (1)110111(1) */
            59, /* x^7+x^6+x^5+x^4    +x^2+x+1 (1)111011(1) */
            -1
        };

        private readonly static long[] AltPrimitivePolynomialDegree08 =
        {
            14,
            56,
            21,
            22,
            38,
            47,
            49,
            50,
            52,
            67,
            70,
            84,
            97,
            103,
            115,
            122,
            -1
        };

        // #define N_ALT_MAX_DEGREE 8;
        private readonly static long[][] AltPrimitivePolynomials =
        {
            AltPrimitivePolynomialDegree01,
            AltPrimitivePolynomialDegree02,
            AltPrimitivePolynomialDegree03,
            AltPrimitivePolynomialDegree04,
            AltPrimitivePolynomialDegree05,
            AltPrimitivePolynomialDegree06,
            AltPrimitivePolynomialDegree07,
            AltPrimitivePolynomialDegree08
        };
       
       private static readonly Lazy<ulong[][]> JoeKuoD7Initializers = new Lazy<ulong[][]>(JoeKuoDirections.JoeKuoD7);
       private static readonly Lazy<ulong[][]> JoeKuoD5Initializers = new Lazy<ulong[][]>(JoeKuoDirections.JoeKuoD5);
       private static readonly Lazy<ulong[][]> JoeKuoD6Initializers = new Lazy<ulong[][]>(JoeKuoDirections.JoeKuoD6);

       private static readonly Lazy<ulong[][]> SLinitializers = new Lazy<ulong[][]>(StdDirections.SLinitializers);
       private static readonly Lazy<ulong[][]> initializers = new Lazy<ulong[][]>(StdDirections.JaeckelInitializers);
       private static readonly Lazy<ulong[][]> Linitializers = new Lazy<ulong[][]>(StdDirections.LemieuxInitializers);

       private static readonly Lazy<ulong[][]> Kuoinitializers = new Lazy<ulong[][]>(KuoDirections.KuoInit);
       private static readonly Lazy<ulong[][]> Kuo3initializers = new Lazy<ulong[][]>(KuoDirections.Kuo3Init);
       private static readonly Lazy<ulong[][]> Kuo2initializers = new Lazy<ulong[][]>(KuoDirections.Kuo2Init);
		
    }
#endif

}
