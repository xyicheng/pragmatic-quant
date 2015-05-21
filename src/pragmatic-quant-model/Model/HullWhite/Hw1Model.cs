using System;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;

namespace pragmatic_quant_model.Model.HullWhite
{
    public class Hw1Model : IModel
    {
        public Hw1Model(ITimeMeasure time, Currency currency, double meanReversion, RrFunction sigma)
        {
            Sigma = sigma;
            MeanReversion = meanReversion;
            Currency = currency;
            Time = time;
        }

        public Currency Currency { get; private set; }
        public double MeanReversion { get; private set; }
        public RrFunction Sigma { get; private set; }

        public ITimeMeasure Time { get; private set; }
        public Currency PivotCurrency
        {
            get { return Currency; }
        }
    }

    public static class HwModelUtils
    {
        public static RrFunction ZcRateCoeffFunction(double maturity, double meanReversion)
        {
            var expMat = Math.Exp(-meanReversion * maturity);
            if (DoubleUtils.MachineEquality(1.0, expMat))
            {
                return RrFunctions.Affine(1.0, -maturity);
            }

            return (expMat / meanReversion) * RrFunctions.Exp(meanReversion) - (1.0 / meanReversion);
        }
        public static double ZcRateCoeff(double duration, double meanReversion)
        {
            return Math.Pow(duration * meanReversion, 2.0) < 6.0 * DoubleUtils.MachineEpsilon
                ? -duration * (1.0 - 0.5 * meanReversion * duration)
                : -(1.0 - Math.Exp(-duration * meanReversion)) / meanReversion;
        }
        public static RnRFunction ZcFunction(double duration, double fwdZc, double[] meanReversions, double[,] covariances)
        {
            var zcRateCoeffs = meanReversions.Map(l => ZcRateCoeff(duration, l));
            double cvx = covariances.BilinearProd(zcRateCoeffs, zcRateCoeffs);
            return RnRFunctions.ExpAffine(fwdZc * Math.Exp(-0.5 * cvx), zcRateCoeffs);
        }
        
        public static RrFunction DriftTerm(this Hw1Model hw1Model)
        {
            return OrnsteinUhlenbeckUtils.IntegratedVariance(hw1Model.Sigma, hw1Model.MeanReversion);
        }
    }
}