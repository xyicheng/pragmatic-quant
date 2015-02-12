using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pragmatic_quant_model.Maths;

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
    }
}
