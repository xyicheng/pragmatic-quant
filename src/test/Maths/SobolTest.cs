using System;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Sobol;

namespace test.Maths
{
    [TestFixture]
    public class SobolTest
    {
        [TestCase(32, SobolDirection.Jaeckel)]
        [TestCase(40, SobolDirection.SobolLevitan)]
        [TestCase(360, SobolDirection.SobolLevitanLemieux)]
        [TestCase(1999, SobolDirection.JoeKuoD5)]
        [TestCase(1799, SobolDirection.JoeKuoD6)]
        [TestCase(1899, SobolDirection.JoeKuoD7)]
        [TestCase(4925, SobolDirection.Kuo)]
        [TestCase(3946, SobolDirection.Kuo2)]
        [TestCase(4586, SobolDirection.Kuo3)]
        public void InitialisationTest(int dim, SobolDirection direction)
        {
            var sobol = new SobolRsg(dim, 0, direction);
            sobol.NextSequence();
            Assert.IsTrue(true);
        }
        
        [Test]
        public void Test1()
        {
            const int dim = 10;
            var sobol = new SobolRsg(dim, 0, SobolDirection.JoeKuoD5);

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

        [TestCase(200, SobolDirection.JoeKuoD5)]
        public void TestCallBachelier(int dim, SobolDirection direction)
        {
            var sobol = new SobolRsg(dim, 0, direction);

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
                    var gaussian = NormalDistribution.FastCumulativeInverse(sample[i]);
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
