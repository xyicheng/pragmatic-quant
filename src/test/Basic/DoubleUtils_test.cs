using Microsoft.VisualStudio.TestTools.UnitTesting;
using pragmatic_quant_model.Basic;

namespace test.Basic
{
    [TestClass]
    public class DoubleUtils_test
    {
        [TestMethod]
        public void TestEqualZero()
        {
            Assert.IsTrue(DoubleUtils.EqualZero(0.0));
            Assert.IsFalse(DoubleUtils.EqualZero(1.0));
            Assert.IsFalse(DoubleUtils.EqualZero(1.0e-28));
        }
    }
}
