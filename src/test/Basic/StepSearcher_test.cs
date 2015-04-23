using System.Linq;
using NUnit.Framework;
using pragmatic_quant_model.Basic;

namespace test.Basic
{
    [TestFixture]
    public class StepSearcher_Test
    {
        [Test]
        public void TestLocateLeftIndex()
        {
            var abs = new[] {0.0, 0.5, 0.7, 1.0, 10.0};
            var stepSearcher = new StepSearcher(abs);

            foreach (var i in Enumerable.Range(0, abs.Length))
            {
                Assert.AreEqual(stepSearcher.LocateLeftIndex(abs[i]), i);
                if (i < abs.Length - 1)
                {
                    double midPoint = 0.5 * (abs[i] + abs[i + 1]);
                    Assert.AreEqual(stepSearcher.LocateLeftIndex(midPoint), i);

                    double nextPointMinus = abs[i + 1] * (1.0 - DoubleUtils.MachineEpsilon);
                    Assert.AreEqual(stepSearcher.LocateLeftIndex(nextPointMinus), i);
                }
            }
            
            Assert.AreEqual(stepSearcher.LocateLeftIndex(abs[0] - 0.00000001), -1);
            Assert.AreEqual(stepSearcher.LocateLeftIndex(abs[abs.Length - 1] + 0.00000001), abs.Length - 1);
            Assert.AreEqual(stepSearcher.LocateLeftIndex(double.NegativeInfinity), -1);
            Assert.AreEqual(stepSearcher.LocateLeftIndex(double.PositiveInfinity), abs.Length - 1);
        }

        [Test]
        public void TestTryFindPillarIndex()
        {
            var abs = new[] {0.1, 0.5004, 0.75, 1.0, 9.99999};
            var stepSearcher = new StepSearcher(abs);

            foreach (var i in Enumerable.Range(0, abs.Length))
            {
                int index;
                Assert.True(stepSearcher.TryFindPillarIndex(abs[i], out index));
                Assert.AreEqual(index, i);

                Assert.True(stepSearcher.TryFindPillarIndex(1.000000000000000000001 * abs[i], out index));
                if (i > 0)
                    Assert.False(stepSearcher.TryFindPillarIndex(0.999999999999 * abs[i], out index));
            }
        }
    }
}
