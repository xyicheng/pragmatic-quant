using System;
using NUnit.Framework;
using pragmatic_quant_model.Basic.Structure;

namespace test.Basic
{
    [TestFixture]
    public class Duration_Test
    {
        [Test]
        public void TestParse()
        {
            Assert.AreEqual(Duration.Parse("5y"), Duration.Year * 5);
            Assert.AreEqual(Duration.Parse("5Y"), 5 * Duration.Year);
            Assert.AreEqual(Duration.Parse("20y"), 20 * Duration.Year);
            Assert.AreEqual(Duration.Parse("20Y"), Duration.Year * 20);
            Assert.AreEqual(Duration.Parse("100y"), 100 * Duration.Year);
            Assert.AreEqual(Duration.Parse("100Y"), Duration.Year * 100);

            Assert.AreEqual(Duration.Parse("5m"), Duration.Month * 5);
            Assert.AreEqual(Duration.Parse("5M"), 5 * Duration.Month);
            Assert.AreEqual(Duration.Parse("20m"), 20 * Duration.Month);
            Assert.AreEqual(Duration.Parse("20M"), Duration.Month * 20);
            Assert.AreEqual(Duration.Parse("100m"), 100 * Duration.Month);
            Assert.AreEqual(Duration.Parse("100M"), Duration.Month * 100);

            Assert.AreEqual(Duration.Parse("5d"), Duration.Day * 5);
            Assert.AreEqual(Duration.Parse("5D"), 5 * Duration.Day);
            Assert.AreEqual(Duration.Parse("20d"), 20 * Duration.Day);
            Assert.AreEqual(Duration.Parse("20D"), Duration.Day * 20);
            Assert.AreEqual(Duration.Parse("100d"), 100 * Duration.Day);
            Assert.AreEqual(Duration.Parse("100D"), Duration.Day * 100);

            Assert.AreEqual(Duration.Parse("5h"), Duration.Hour * 5);
            Assert.AreEqual(Duration.Parse("5H"), 5 * Duration.Hour);
            Assert.AreEqual(Duration.Parse("20h"), 20 * Duration.Hour);
            Assert.AreEqual(Duration.Parse("20H"), Duration.Hour * 20);
            Assert.AreEqual(Duration.Parse("100h"), 100 * Duration.Hour);
            Assert.AreEqual(Duration.Parse("100H"), Duration.Hour * 100);
        }

        [Test]
        public void TestDateAdd()
        {
            var refDate = new DateTime(1978, 2, 22);
            Assert.AreEqual(refDate + 37 * Duration.Year, new DateTime(2015, 2, 22));
            Assert.AreEqual(refDate + 18 * Duration.Month, new DateTime(1979, 8, 22));
            Assert.AreEqual(refDate + 5 * Duration.Day, new DateTime(1978, 2, 27));
        }
    }
}
