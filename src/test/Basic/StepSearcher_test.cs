using System.Linq;
using NUnit.Framework;
using pragmatic_quant_model.Basic;

namespace test.Basic
{
    [TestFixture]
    public class StepSearcherTest
    {
        [Test]
        public void TestLocateLeftIndex()
        {
            var pillars = new[] {0.0, 0.5, 0.7, 1.0, 10.0};
            var stepSearcher = new StepSearcher(pillars);

            foreach (var i in Enumerable.Range(0, pillars.Length))
            {
                Assert.AreEqual(stepSearcher.LocateLeftIndex(pillars[i]), i);
                if (i < pillars.Length - 1)
                {
                    double midPoint = 0.5 * (pillars[i] + pillars[i + 1]);
                    Assert.AreEqual(stepSearcher.LocateLeftIndex(midPoint), i);

                    double nextPointMinus = pillars[i + 1] * (1.0 - DoubleUtils.MachineEpsilon);
                    Assert.AreEqual(stepSearcher.LocateLeftIndex(nextPointMinus), i);
                }
            }
            
            Assert.AreEqual(stepSearcher.LocateLeftIndex(pillars[0] - 0.00000001), -1);
            Assert.AreEqual(stepSearcher.LocateLeftIndex(pillars[pillars.Length - 1] + 0.00000001), pillars.Length - 1);
            Assert.AreEqual(stepSearcher.LocateLeftIndex(double.NegativeInfinity), -1);
            Assert.AreEqual(stepSearcher.LocateLeftIndex(double.PositiveInfinity), pillars.Length - 1);
        }

        [Test]
        public void TestTryFindPillarIndex()
        {
            var pillars = new[] {0.1, 0.5004, 0.75, 1.0, 9.99999};
            var stepSearcher = new StepSearcher(pillars);

            foreach (var i in Enumerable.Range(0, pillars.Length))
            {
                int index;
                Assert.True(stepSearcher.TryFindPillarIndex(pillars[i], out index));
                Assert.AreEqual(index, i);

                Assert.True(stepSearcher.TryFindPillarIndex(1.000000000000000000001 * pillars[i], out index));
                Assert.False(stepSearcher.TryFindPillarIndex(0.999999999999 * pillars[i], out index));
            }

            int indexTest;
            Assert.False(stepSearcher.TryFindPillarIndex(double.NegativeInfinity, out indexTest));
            Assert.False(stepSearcher.TryFindPillarIndex(double.PositiveInfinity, out indexTest));
        }
    }
}
