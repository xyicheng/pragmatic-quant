using NUnit.Framework;
using pragmatic_quant_model.Basic;

namespace test.Basic
{
    [TestFixture]
    public class GriddUtils_Test
    {
        [Test]
        public void TestRegularGrid()
        {
            var g = GridUtils.RegularGrid(0.0, 1.0, 4);
            
        }
    }
}
