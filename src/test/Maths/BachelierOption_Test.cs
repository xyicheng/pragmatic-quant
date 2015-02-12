using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths;

namespace test.Maths
{
    [TestClass]
    public class BachelierOption_Test
    {
        [TestMethod]
        public void CallNaiveImplemXCheck()
        {
            var maturities = GridUtils.RegularGrid(0.1, 5.0, 100);
            var vols = GridUtils.RegularGrid(0.001, 0.015, 10);
            var volMoneynesses = GridUtils.RegularGrid(-3, 3, 100);

            foreach (var mat in maturities)
            {
                foreach (var vol in vols)
                {
                    var sigma = Math.Sqrt(mat) * vol;
                    foreach (var vm in volMoneynesses)
                    {
                        var strike = vm * sigma;
                        var call = BachelierOption.Price(0.0, strike, vol, mat, 1);

                        //Naive implementation of bachelier formulae
                        var d = strike / sigma;
                        var call_xcheck = -strike * NormalDistribution.Cumulative(-d) + sigma * NormalDistribution.Density(d);

                        if (DoubleUtils.EqualZero(call)) Assert.IsTrue(DoubleUtils.EqualZero(call_xcheck));
                        else
                        {
                            var errRelative = (call - call_xcheck) / call_xcheck;
                            Assert.IsTrue(Math.Abs(errRelative) < 1.0e-13);
                        }
                    }
                }
            }
        }

        [TestMethod]
        public void ImpliedVol_Test()
        {
            const double mat = 0.5;
            const double vol = 0.070;
            var moneynesses = GridUtils.RegularGrid(-5.5, 5.5, 10000);

            foreach (double m in moneynesses)
            {
                var strike = m * vol * Math.Sqrt(mat);
                var option = BachelierOption.Price(0.0, strike, vol, mat, m > 0 ? 1 : -1);
                var impliedVol = BachelierOption.ImpliedVol(option, 0.0, strike, mat, m > 0 ? 1 : -1);

                var errRelative = (impliedVol - vol) / vol;
                Assert.IsTrue(Math.Abs(errRelative) < 5.5e-12);
            }
        }

        [TestMethod]
        public void ImpliedVol_NearForward_Test()
        {
            const double mat = 0.5;
            const double vol = 0.070;
            var moneynesses = GridUtils.RegularGrid(-1.0e-6, 1.0e-6, 10000);

            foreach (double m in moneynesses)
            {
                var strike = m * vol * Math.Sqrt(mat);
                var option = BachelierOption.Price(0.0, strike, vol, mat, m > 0 ? 1 : -1);
                var impliedVol = BachelierOption.ImpliedVol(option, 0.0, strike, mat, m > 0 ? 1 : -1);

                var errRelative = (impliedVol - vol) / vol;
                Assert.IsTrue(Math.Abs(errRelative) < 5.0e-12);
            }
        }

        [TestMethod]
        public void ImpliedVol_HighRegion_Test()
        {
            const double mat = 0.5;
            const double vol = 0.070;
            var moneynesses = GridUtils.RegularGrid(5.5, 15.0, 10000);

            foreach (double m in moneynesses)
            {
                var strike = m * vol * Math.Sqrt(mat);
                var option = BachelierOption.Price(0.0, strike, vol, mat, m > 0 ? 1 : -1);
                var impliedVol = BachelierOption.ImpliedVol(option, 0.0, strike, mat, m > 0 ? 1 : -1);

                var errRelative = (impliedVol - vol) / vol;
                Assert.IsTrue(Math.Abs(errRelative) < 5.5e-11);
            }
        }

        [TestMethod]
        public void ImpliedVol_VeryHighRegion_Test()
        {
            const double mat = 0.5;
            const double vol = 0.070;
            var moneynesses = GridUtils.RegularGrid(15.0, 37.0, 10000); 

            foreach (double m in moneynesses)
            {
                var strike = m * vol * Math.Sqrt(mat);
                var option = BachelierOption.Price(0.0, strike, vol, mat, m > 0 ? 1 : -1);
                var impliedVol = BachelierOption.ImpliedVol(option, 0.0, strike, mat, m > 0 ? 1 : -1);

                var errRelative = (impliedVol - vol) / vol;
                Assert.IsTrue(Math.Abs(errRelative) < 6.0e-12);
            }
        }
    }
}
