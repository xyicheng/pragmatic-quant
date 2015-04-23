using System;
using NUnit.Framework;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Maths;

namespace test.Maths
{
    [TestFixture]
    public class BlackOption_Test
    {
        [Test]
        public void CallNaiveImplemXCheck()
        {
            var maturities = GridUtils.RegularGrid(0.1, 5.0, 100);
            var vols = GridUtils.RegularGrid(0.01, 1.0, 10);
            var volMoneynesses = GridUtils.RegularGrid(-3, 3, 100);

            foreach (var mat in maturities)
            {
                foreach (var vol in vols)
                {
                    var sigma = Math.Sqrt(mat) * vol;
                    foreach (var vm in volMoneynesses)
                    {
                        var strike = Math.Exp(vm * sigma);
                        var call = BlackScholesOption.Price(1.0, strike, vol, mat, 1);
                        
                        //Naive implementation of black-scholes formulae
                        var d_plus = -(vm * sigma) / sigma + 0.5 * sigma;
                        var d_minus = d_plus - sigma;
                        var call_xcheck = NormalDistribution.Cumulative(d_plus) - strike * NormalDistribution.Cumulative(d_minus);

                        if (DoubleUtils.EqualZero(call)) Assert.IsTrue(DoubleUtils.EqualZero(call_xcheck));
                        else
                        {
                            var errRelative = (call - call_xcheck) / call_xcheck;
                            Assert.IsTrue(Math.Abs(errRelative) < 1.0e-11);
                        }
                    }
                }
            }
        }
        
        [Test]
        public void ImpliedVol_MoneynessSample_Test()
        {
            const double mat = 0.5;
            const double vol = 0.253251;
            var moneynesses = GridUtils.RegularGrid(-5.0, 5.0, 100);
            
            foreach(double m in moneynesses)
            {
                var strike = Math.Exp(m);
                var option = BlackScholesOption.Price(1.0, strike, vol, mat, m > 0 ? 1 : -1);
                var impliedVol = BlackScholesOption.ImpliedVol(option, 1.0, strike, mat, m > 0 ? 1 : -1);

                var errRelative = (impliedVol - vol) / vol;
                Assert.IsTrue(Math.Abs(errRelative) < 6 * DoubleUtils.MachineEpsilon);
            }
        }

        [Test]
        public void Implied_VolSample_Test()
        {
            const double mat = 1.0;
            const double strike = 1.50;
            var vols = GridUtils.RegularGrid(0.015, 2.0, 100);

            foreach (double vol in vols)
            {
                var option = BlackScholesOption.Price(1.0, strike, vol, mat, 1);
                var impliedVol = BlackScholesOption.ImpliedVol(option, 1.0, strike, mat, 1);

                var errRelative = (impliedVol - vol) / vol;
                Assert.IsTrue(Math.Abs(errRelative) < 6 * DoubleUtils.MachineEpsilon);
            }
        }

    }
}
