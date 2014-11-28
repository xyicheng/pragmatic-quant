using Microsoft.VisualStudio.TestTools.UnitTesting;
using pragmatic_quant_model.Basic;

namespace test.Basic
{
    [TestClass]
    public class StepSearcher_test
    {
        [TestMethod]
        public void TestLocateLeftIndex()
        {
            var stepSearcher = new StepSearcher(new[] {0.0, 0.5, 0.7, 1.0, 10.0});

            Assert.AreEqual(stepSearcher.LocateLeftIndex(0.0), 0);
            Assert.AreEqual(stepSearcher.LocateLeftIndex(0.5), 1);
            Assert.AreEqual(stepSearcher.LocateLeftIndex(0.7), 2);
            Assert.AreEqual(stepSearcher.LocateLeftIndex(1.0), 3);
            Assert.AreEqual(stepSearcher.LocateLeftIndex(10.0), 4);

            Assert.AreEqual(stepSearcher.LocateLeftIndex(-0.1), -1);
            Assert.AreEqual(stepSearcher.LocateLeftIndex(0.25), 0);
            Assert.AreEqual(stepSearcher.LocateLeftIndex(1.01), 3);
            Assert.AreEqual(stepSearcher.LocateLeftIndex(11.0), 4);
        }
    }
}
