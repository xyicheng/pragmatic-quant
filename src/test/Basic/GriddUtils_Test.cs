using Microsoft.VisualStudio.TestTools.UnitTesting;
using pragmatic_quant_model.Basic;

namespace test.Basic
{
    [TestClass]
    public class GriddUtils_Test
    {
        [TestMethod]
        public void TestRegularGrid()
        {
            var g = GridUtils.RegularGrid(0.0, 1.0, 4);
            
        }
    }
}
