using System;
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

            double[] atmCalls = new double[dim];
            double[] strike1Call = new double[dim];
            double[] strike2Call = new double[dim];
            const int nbPaths = 1000000;
            for (int index = 0; index < nbPaths; index++)
            {
                var sample = sobol.NextSequence();
                for (int i = 0; i < dim; i++)
                {
                    var gaussian = NormalDistribution.CumulativeInverse(sample[i]);
                    atmCalls[i] += Math.Max(0.0, gaussian);
                    strike1Call[i] += Math.Max(0.0, gaussian - 1.0);
                    strike2Call[i] += Math.Max(0.0, gaussian - 2.0);
                }
            }
            for (int i = 0; i < dim; i++)
            {
                atmCalls[i] /= nbPaths;
                strike1Call[i] /= nbPaths;
                strike2Call[i] /= nbPaths;
            }

            var refAtmCalls = atmCalls.Select(c => BachelierOption.Price(0.0, 0.0, 1.0, 1.0, 1.0)).ToArray();
            var refStrike1Calls = atmCalls.Select(c => BachelierOption.Price(0.0, 1.0, 1.0, 1.0, 1.0)).ToArray();
            var refStrike2Calls = atmCalls.Select(c => BachelierOption.Price(0.0, 2.0, 1.0, 1.0, 1.0)).ToArray();
            
            UnitTestUtils.EqualDoubleArray(atmCalls, refAtmCalls, 2.0e-5, true);
            UnitTestUtils.EqualDoubleArray(strike1Call, refStrike1Calls, 8.0e-5, true);
            UnitTestUtils.EqualDoubleArray(strike2Call, refStrike2Calls, 6.0e-4, true);
        }
    }
}
