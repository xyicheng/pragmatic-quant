using System;
using System.Globalization;
using NUnit.Framework;
using pragmatic_quant_com.Factories;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Structure;

namespace test
{
    [TestFixture]
    public class BagServices_Test
    {
        [Test]
        public void TestHas()
        {
            var bag1 = new object[,]
                {
                    {null, null, null},
                    {null, null, "ToTo"}
                };
            int row, col;
            bool hasToto1 = BagServices.Has(bag1, "Toto", out row, out col);
            bool hasToto2 = BagServices.Has(bag1, " tOtO ");

            Assert.IsTrue(hasToto1);
            Assert.IsTrue(hasToto2);
            Assert.AreEqual(row, 1);
            Assert.AreEqual(col, 2);
        }

        [Test]
        public void TestProcessScalarString()
        {
            var bag = new object[,]
                {
                    {null, null, null},
                    {"", "toto ", "hello"},
                    {null, null, ""}
                };

            var totoVal = BagServices.ProcessScalarString(bag, "Toto");
            Assert.AreEqual(totoVal, "hello");
        }

        [Test]
        public void TestProcessScalarDouble()
        {
            //Test 1
            var bag1 = new object[,]
                {
                    {null, null, null},
                    {"", "Toto", 3.14159265},
                    {null, null, ""}
                };
            var totoDoubleVal1 = BagServices.ProcessScalarDouble(bag1, "Toto");
            Assert.AreEqual(totoDoubleVal1, 3.14159265);

            //Test2
            var bag2 = new object[,]
                {
                    {null, null, null},
                    {"", "Toto", 3.14159265.ToString(CultureInfo.InvariantCulture)},
                    {null, null, ""}
                };
            var totoDoubleVal2 = BagServices.ProcessScalarDouble(bag2, "Toto");
            Assert.AreEqual(totoDoubleVal2, 3.14159265);
        }

        [Test]
        public void TestProcessScalarDate()
        {
            //Test1
            var date = new DateTime(2014, 1, 05);
            var bag1 = new object[,]
                {
                    {null, null, null},
                    {"", "Toto", date},
                    {null, null, ""}
                };
            var totoDateVal1 = BagServices.ProcessScalarDateOrDuration(bag1, "Toto");
            Assert.AreEqual(totoDateVal1.Date, date);

            //Test2
            var bag2 = new object[,]
                {
                    {null, null, null},
                    {"", "Toto", date.ToString(CultureInfo.InvariantCulture) },
                    {null, null, ""}
                };
            var totoDateVal2 = BagServices.ProcessScalarDateOrDuration(bag2, "Toto");
            Assert.AreEqual(totoDateVal2.Date, date);
        }

        [Test]
        public void TestProcessScalarDurationYear()
        {
            var dur = 10 * Duration.Year;

            //Test1
            var bag1 = new object[,]
                {
                    {null, null, null},
                    {"", "Toto", "10y "},
                    {null, null, ""}
                };
            var totoVal1 = BagServices.ProcessScalarDateOrDuration(bag1, "Toto");
            Assert.AreEqual(totoVal1.Duration, dur);

            //Test2
            var bag2 = new object[,]
                {
                    {null, null, null},
                    {"", " Toto", dur},
                    {null, null, ""}
                };
            var totoVal2 = BagServices.ProcessScalarDateOrDuration(bag2, "Toto");
            Assert.AreEqual(totoVal2.Duration, dur);

            //Test3
            var bag3 = new object[,]
                {
                    {null, null, null},
                    {"", " Toto", dur.ToString()},
                    {null, null, ""}
                };
            var totoVal3 = BagServices.ProcessScalarDateOrDuration(bag3, "Toto");
            Assert.AreEqual(totoVal3.Duration, dur);
        }

        [Test]
        public void TestProcessScalarDurationMonth()
        {
            var dur = 112 * Duration.Month;

            //Test1
            var bag1 = new object[,]
                {
                    {null, null, null},
                    {"", "Toto", " 112m "},
                    {null, null, ""}
                };
            var totoVal1 = BagServices.ProcessScalarDateOrDuration(bag1, "Toto");
            Assert.AreEqual(totoVal1.Duration, dur);

            //Test2
            var bag2 = new object[,]
                {
                    {null, null, null},
                    {"", " Toto", dur},
                    {null, null, ""}
                };
            var totoVal2 = BagServices.ProcessScalarDateOrDuration(bag2, "Toto");
            Assert.AreEqual(totoVal2.Duration, dur);

            //Test3
            var bag3 = new object[,]
                {
                    {null, null, null},
                    {"", " Toto", dur.ToString()},
                    {null, null, ""}
                };
            var totoVal3 = BagServices.ProcessScalarDateOrDuration(bag3, "Toto");
            Assert.AreEqual(totoVal3.Duration, dur);
        }

        [Test]
        public void TestProcessScalarDurationDay()
        {
            var dur = 30 * Duration.Day;

            //Test1
            var bag1 = new object[,]
                {
                    {null, null, null},
                    {"", "Toto", " 30d "},
                    {null, null, ""}
                };
            var totoVal1 = BagServices.ProcessScalarDateOrDuration(bag1, "Toto");
            Assert.AreEqual(totoVal1.Duration, dur);

            //Test2
            var bag2 = new object[,]
                {
                    {null, null, null},
                    {"", " Toto", dur},
                    {null, null, ""}
                };
            var totoVal2 = BagServices.ProcessScalarDateOrDuration(bag2, "Toto");
            Assert.AreEqual(totoVal2.Duration, dur);

            //Test3
            var bag3 = new object[,]
                {
                    {null, null, null},
                    {"", " Toto", dur.ToString()},
                    {null, null, ""}
                };
            var totoVal3 = BagServices.ProcessScalarDateOrDuration(bag3, "Toto");
            Assert.AreEqual(totoVal3.Duration, dur);
        }

        [Test]
        public void TestVectorString()
        {

            //Test1
            var bag1 = new object[,]
                {
                    {null, null, null},
                    {"", "myparam ", "hello"},
                    {null, "val1", ""},
                    {null, "val2", ""},
                    {null, "val3", ""}
                };
            var myParamArray1 = BagServices.ProcessVectorString(bag1, "MyParam");
            UnitTestUtils.EqualArrays(myParamArray1, new[] { "val1", "val2", "val3" });

            //Test2
            var bag2 = new object[,]
                {
                    {null, null, null},
                    {"", "myparam ", "hello"},
                    {null, "val1", ""},
                    {null, "val2", ""},
                    {null, null, ""}
                };
            var myParamArray2 = BagServices.ProcessVectorString(bag2, "MyParam");
            UnitTestUtils.EqualArrays(myParamArray2, new[] { "val1", "val2" });

            //Test3
            var bag3 = new object[,]
                {
                    {null, null, null},
                    {"", "myparam ", "hello"},
                    {null, "val1", ""},
                    {null, "val2", ""},
                    {null, null, ""}
                    //{null, ExcelDna.Integration.ExcelEmpty.Value, ""}
                };
            var myParamArray3 = BagServices.ProcessVectorString(bag3, "MyParam");
            UnitTestUtils.EqualArrays(myParamArray3, new[] { "val1", "val2" });

            //Test4
            var bag4 = new object[,]
                {
                    {null, null, null},
                    {"", "myparam ", "hello"},
                    {null, "val1", ""},
                    {null, "val2", ""},
                    {null, "", ""}
                };
            var myParamArray4 = BagServices.ProcessVectorString(bag4, "MyParam");
            UnitTestUtils.EqualArrays(myParamArray4, new[] { "val1", "val2" });

            //Test5
            var bag5 = new object[,]
                {
                    {null, null, null},
                    {"", "myparam ", "hello"},
                    {null, "val1", ""},
                    {null, "val2", ""},
                    {null, "   ", ""}
                };
            var myParamArray5 = BagServices.ProcessVectorString(bag5, "MyParam");
            UnitTestUtils.EqualArrays(myParamArray5, new[] { "val1", "val2" });
        }
        
        [Test]
        public void TestMatrixString()
        {
            //Test1
            var bag1 = new object[,]
                {
                    {null, null, null},
                    {"", "myparam ", "hello"},
                    {null, "val11", "val12"},
                    {null, "val21", "val22"},
                    {null, "val31", "val32"}
                };
            var myParamArray1 = BagServices.ProcessMatrixString(bag1, "MyParam");
            UnitTestUtils.EqualMatrix(myParamArray1, new[,] {{"val11", "val12"}, {"val21", "val22"}, {"val31", "val32"}});

            //Test2
            var bag2 = new object[,]
                {
                    {null, null, null, null},
                    {"", "myparam ", "hello", ""},
                    {null, "val11", "val12", ""},
                    {null, "val21", "val22", ""},
                    {null, null, "", ""}
                };
            var myParamArray2 = BagServices.ProcessMatrixString(bag2, "MyParam");
            UnitTestUtils.EqualMatrix(myParamArray2, new[,] {{"val11", "val12"}, {"val21", "val22"}});
        }

        [Test]
        public void TestTimeMatrixDouble()
        {
            //Test1
            var bag1 = new object[,]
            {
                {null, null, null, null},
                {"", "myparam ", "hello", "toto"},
                {null, "1y", "1.0", 5.0},
                {null, "02/22/1978", "2.0", "4.0"},
                {null, "10y", "3.0", 1.0}
            };
            
            TimeMatrixDatas timeDatas = BagServices.ProcessTimeMatrixDatas(bag1, "MyParam");
            var helloDatas = timeDatas.GetCol("hello");
            var totoDatas = timeDatas.GetCol("toto");
            
            UnitTestUtils.EqualDoubleArray(helloDatas, new[] {1.0, 2.0, 3.0}, DoubleUtils.MachineEpsilon);
            UnitTestUtils.EqualDoubleArray(totoDatas, new[] { 5.0, 4.0, 1.0 }, DoubleUtils.MachineEpsilon);
        }
    }
}
