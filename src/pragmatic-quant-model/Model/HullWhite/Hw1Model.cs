﻿using System;
using System.Diagnostics.Contracts;
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
    }

    public static class HwModelUtils
    {
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

    public class Hw1ModelZcRepresentation : RateZcRepresentation
    {
        #region private fields
        private readonly ITimeMeasure time;
        private readonly double meanReversion;
        private readonly RrFunction driftTerm;
        #endregion
        public Hw1ModelZcRepresentation(Hw1Model hw1Model)
            : base(hw1Model.Currency)
        {
            time = hw1Model.Time;
            meanReversion = hw1Model.MeanReversion;
            driftTerm = hw1Model.DriftTerm();
        }
        public override RnRFunction Zc(DateTime date, DateTime maturity, double fwdZc)
        {
            Contract.Requires(date <= maturity);
            double d = time[date];
            var drift = driftTerm.Eval(d);
            return HwModelUtils.ZcFunction(time[maturity] - d, fwdZc, new[] {meanReversion}, new[,] {{drift}});
        }
    }
}