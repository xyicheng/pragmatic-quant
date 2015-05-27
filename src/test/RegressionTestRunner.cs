using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Microsoft.Office.Interop.Excel;
using pragmatic_quant_model.Basic;
using Excel = Microsoft.Office.Interop.Excel.Application;
using Worksheet = Microsoft.Office.Interop.Excel.Worksheet;

namespace test
{
    [TestFixture]
    public class RegressionTestRunner
    {
        #region Private Fields
        private Excel xl;
        #endregion

        #region SetUp/TearDown
        [SetUp]
        public void SetUp()
        {
            xl = new Excel { Visible = false };
            var xllPath = Path.Combine(Environment.CurrentDirectory, @"pragmatic-quant.xll");
            xl.RegisterXLL(xllPath);
        }

        [TearDown]
        public void TearDown()
        {
            xl.Quit();
        }
        #endregion
        
        #region Private Methods
        private static Worksheet GetSheet(Workbook wbk, string name)
        {
            var sheet = wbk.Sheets
                .Cast<Worksheet>()
                .FirstOrDefault(sht => sht.Name.ToLower().Equals(name.ToLower()));
            if (sheet == null)
                throw new Exception(string.Format(@"Missing worksheet {0}", name));
            return sheet;
        }
        private static void AssertRangeEquality(object[,] rng, object[,] refRng, double tolerance)
        {
            Assert.AreEqual(rng.GetLength(0), refRng.GetLength(0));
            Assert.AreEqual(rng.GetLength(1), refRng.GetLength(1));

            for (int i = rng.GetLowerBound(0); i <= rng.GetUpperBound(0); i++)
            {
                for (int j = rng.GetLowerBound(1); j <= rng.GetUpperBound(1); j++)
                {
                    double value, refValue;
                    if (!(NumberConverter.TryConvertDouble(rng[i, j], out value)
                          && NumberConverter.TryConvertDouble(refRng[i, j], out refValue)))
                        throw new Exception(string.Format(" tested value is not a double"));
                    var error = Math.Abs(value - refValue);
                    Assert.LessOrEqual(error, tolerance);
                }
            }
        }
        #endregion
        
        [Test, TestCaseSource(typeof(TestWorkbookFactory), "Worbooks")]
        public void TestWorkbook(string path)
        {
            var wbk = xl.Workbooks.Open(path);
            try
            {
                var testSheet = GetSheet(wbk, "Test");

                for (int i = 0; i < 100; i++)
                {
                    
                    var sheetName = testSheet.Cells[2 + i, 1].Value as string;
                    if (sheetName == null)
                        break;
                    
                    Console.WriteLine("Testing {0}...", sheetName);

                    var action = testSheet.Cells[2 + i, 2].Value as string ?? "";
                    var rangeName = testSheet.Cells[2 + i, 3].Value as string;
                    var refSheetName = testSheet.Cells[2 + i, 4].Value as string;
                    var refRangeName = testSheet.Cells[2 + i, 5].Value as string;

                    double tolerance;
                    NumberConverter.TryConvertDouble(testSheet.Cells[2 + i, 6].Value, out tolerance);
                    
                    var sheet = GetSheet(wbk, sheetName);
                    switch (action.ToLower().Trim())
                    {
                        case "sheetcalculate":
                            sheet.Calculate();
                            break;
                        case "":
                            break;
                        default:
                            throw new Exception(string.Format("Unknow test action {0}", action));
                    }

                    Console.WriteLine("   Do {0}", action);

                    var range = sheet.Range[rangeName].Value;
                    var refSheet = GetSheet(wbk, refSheetName);
                    var refRange = refSheet.Range[refRangeName].Value;

                    Console.WriteLine("   Compare range {0} with {1}!{2}",
                        rangeName, refSheetName, refRangeName);
                    AssertRangeEquality(range, refRange, tolerance);
                    Console.WriteLine("Succeed !");
                }
                
                wbk.Close(false);
            }
            catch (Exception e)
            {
                wbk.Close(false);
                Console.WriteLine(e.Message);
                Assert.Fail();
            }
        }
    }

    public class TestWorkbookFactory
    {
        public static IEnumerable Worbooks
        {
            get
            {
                var wbkListPath = Path.Combine(Environment.CurrentDirectory, @"..\..\Regression\WorkbookList.txt");
                using (var wbkListFile = new StreamReader(wbkListPath))
                {
                    string fileName;
                    while ((fileName = wbkListFile.ReadLine()) != null)
                    {
                        var path = string.Format(@"..\..\Regression\{0}", fileName);
                        var fullPath = Path.Combine(Environment.CurrentDirectory, path);

                        yield return new TestCaseData(fullPath).SetName(fileName);

                    }
                }
            }
        }
    }

}
