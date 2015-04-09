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

                    double nextPointMinus = abs[i] + (1.0 - 10.0 * DoubleUtils.Epsilon) * (abs[i + 1] - abs[i]);
                    Assert.AreEqual(stepSearcher.LocateLeftIndex(nextPointMinus), i);
                }
            }
            
            Assert.AreEqual(stepSearcher.LocateLeftIndex(abs[0] - 0.00000001), -1);
            Assert.AreEqual(stepSearcher.LocateLeftIndex(abs[abs.Length - 1] + 0.00000001), abs.Length - 1);
            Assert.AreEqual(stepSearcher.LocateLeftIndex(double.NegativeInfinity), -1);
            Assert.AreEqual(stepSearcher.LocateLeftIndex(double.PositiveInfinity), abs.Length - 1);
        }
    }
}
