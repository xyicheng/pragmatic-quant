using System;
using NUnit.Framework;
using pragmatic_quant_model.Basic;

namespace test.Basic
{
    [TestFixture]
    public class DoubleUtils_Test
    {
        [Test]
        public void TestEqualZero()
        {
            Assert.IsTrue(DoubleUtils.EqualZero(0.0));
            Assert.IsFalse(DoubleUtils.EqualZero(1.0));
            Assert.IsFalse(DoubleUtils.EqualZero(1.0e-28));
        }

        [Test]
        public void TestMachineEquality()
        {
            Assert.IsTrue(DoubleUtils.MachineEquality(1.0 + DoubleUtils.Epsilon, 1.0));
            Assert.IsTrue(DoubleUtils.MachineEquality(1.00000000000000000000001, 1.0));
            Assert.IsFalse(DoubleUtils.MachineEquality(1.0 + 2.0 * DoubleUtils.Epsilon, 1.0));
            Assert.IsFalse(DoubleUtils.MachineEquality(1.0e-28, 0.0));

            var rand = new Random(255);
            for (int i = 0; i < 1e6; i++)
            {
                var x = rand.NextDouble();
                Assert.IsTrue(DoubleUtils.MachineEquality(x, x));
                Assert.IsTrue(DoubleUtils.MachineEquality(-x, -x));
                Assert.IsTrue(DoubleUtils.MachineEquality(100.0 * x, 100.0 * x));
                Assert.IsTrue(DoubleUtils.MachineEquality(-100.0 * x, -100.0 * x));
                Assert.IsTrue(DoubleUtils.MachineEquality(10000000000.0 * x, 10000000000.0 * x));
                Assert.IsTrue(DoubleUtils.MachineEquality(-10000000000.0 * x, -10000000000.0 * x));
            }
        }
    }
}
