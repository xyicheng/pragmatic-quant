using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Sobol;

namespace test.Maths
{
    [TestClass]
    public class Sobol_Test
    {
        [TestMethod]
        public void InitialisationTest()
        {
            var chrono = new Stopwatch();
            chrono.Start();

            var jaeckel = new SobolRsg(32, 0, SobolRsg.DirectionIntegers.Jaeckel);
            jaeckel.NextSequence();

            var sobolLevitan = new SobolRsg(40, 0, SobolRsg.DirectionIntegers.SobolLevitan);
            sobolLevitan.NextSequence();

            var sobolLevitanLemieux = new SobolRsg(360, 0, SobolRsg.DirectionIntegers.SobolLevitanLemieux);
            sobolLevitanLemieux.NextSequence();

            var joeKuoD5 = new SobolRsg(1999, 0, SobolRsg.DirectionIntegers.JoeKuoD5);
            joeKuoD5.NextSequence();

            var joeKuoD6 = new SobolRsg(1799, 0, SobolRsg.DirectionIntegers.JoeKuoD6);
            joeKuoD6.NextSequence();

            var joeKuoD7 = new SobolRsg(1899, 0, SobolRsg.DirectionIntegers.JoeKuoD7);
            joeKuoD7.NextSequence();

            var kuo = new SobolRsg(4925, 0, SobolRsg.DirectionIntegers.Kuo);
            kuo.NextSequence();

            var kuo2 = new SobolRsg(3946, 0, SobolRsg.DirectionIntegers.Kuo2);
            kuo2.NextSequence();

            var kuo3 = new SobolRsg(4586, 0, SobolRsg.DirectionIntegers.Kuo3);
            kuo3.NextSequence();

            chrono.Stop();
            Console.WriteLine("Elapsed " + chrono.Elapsed);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test1()
        {
            const int dim = 10;
            var sobol = new SobolRsg(dim, 0, SobolRsg.DirectionIntegers.JoeKuoD5);

            var rand = new Random(255);
            var cubeSup = Enumerable.Range(0, dim).Select(i => 0.7 + 0.3 * rand.NextDouble()).ToArray();

            var estimatedVolume = 0.0;

            const int nbPaths = 1000000;
            for (int index = 0; index < nbPaths; index++)
            {
                var sample = sobol.NextSequence();
                bool cubeContainSample = !sample.Where((t, i) => t > cubeSup[i]).Any();
                estimatedVolume += cubeContainSample ? 1.0 : 0.0;
            }
            estimatedVolume /= nbPaths;

            var trueVolume = cubeSup.Aggregate(1.0, (previous, c) => previous * c);
            Assert.IsTrue(Math.Abs((estimatedVolume - trueVolume) / trueVolume) < 1.0e-5);
        }

        [TestMethod]
        public void TestCallBachelier()
        {
            const int dim = 200;
            var sobol = new SobolRsg(dim, 0, SobolRsg.DirectionIntegers.JoeKuoD5);

            var chrono = new Stopwatch();
            chrono.Start();    

            var atmCalls = new double[dim];
            var strikeCall1 = new double[dim];
            var strikeCall2 = new double[dim];
            var strikePut1 = new double[dim];
            var strikePut2 = new double[dim];
            const int nbPaths = 1000000;
            for (int index = 0; index < nbPaths; index++)
            {
                var sample = sobol.NextSequence();
                for (int i = 0; i < dim; i++)
                {
                    var gaussian = NormalDistribution.CumulativeInverse(sample[i]);
                    atmCalls[i] += Math.Max(0.0, gaussian);
                    strikeCall1[i] += Math.Max(0.0, gaussian - 1.0);
                    strikeCall2[i] += Math.Max(0.0, gaussian - 2.0);
                    strikePut1[i] += Math.Max(0.0, -1.0 - gaussian);
                    strikePut2[i] += Math.Max(0.0, -2.0 - gaussian);
                }
            }
            for (int i = 0; i < dim; i++)
            {
                atmCalls[i] /= nbPaths;
                strikeCall1[i] /= nbPaths;
                strikeCall2[i] /= nbPaths;
                strikePut1[i] /= nbPaths;
                strikePut2[i] /= nbPaths;
            }

            chrono.Stop();
            Console.WriteLine("Elapsed " + chrono.Elapsed);

            var refAtmCalls = atmCalls.Select(c => BachelierOption.Price(0.0, 0.0, 1.0, 1.0, 1)).ToArray();
            var refStrikeCall1 = atmCalls.Select(c => BachelierOption.Price(0.0, 1.0, 1.0, 1.0, 1)).ToArray();
            var refStrikeCall2 = atmCalls.Select(c => BachelierOption.Price(0.0, 2.0, 1.0, 1.0, 1)).ToArray();
            var refStrikePut1 = atmCalls.Select(c => BachelierOption.Price(0.0, -1.0, 1.0, 1.0, -1)).ToArray();
            var refStrikePut2 = atmCalls.Select(c => BachelierOption.Price(0.0, -2.0, 1.0, 1.0, -1)).ToArray();
            
            UnitTestUtils.EqualDoubleArray(atmCalls, refAtmCalls, 2.0e-5, true);
            UnitTestUtils.EqualDoubleArray(strikeCall1, refStrikeCall1, 8.0e-5, true);
            UnitTestUtils.EqualDoubleArray(strikeCall2, refStrikeCall2, 6.0e-4, true);
            UnitTestUtils.EqualDoubleArray(strikePut1, refStrikePut1, 8.0e-5, true);
            UnitTestUtils.EqualDoubleArray(strikePut2, refStrikePut2, 6.0e-4, true);
        }
    }
}
