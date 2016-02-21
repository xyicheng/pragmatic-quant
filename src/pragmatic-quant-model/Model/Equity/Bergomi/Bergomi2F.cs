using System;
using System.Diagnostics;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Function;
using pragmatic_quant_model.Model.Equity.Dividends;

namespace pragmatic_quant_model.Model.Equity.Bergomi
{
    public class Bergomi2FModelDescription : IModelDescription
    {
        public Bergomi2FModelDescription(string asset, bool withDivs, MapRawDatas<DateOrDuration, double> sigma, 
            double k1, double k2, double theta, double nu, double rhoXy, double rhoSx, double rhoSy)
        {
            Asset = asset;
            WithDivs = withDivs;
            Sigma = sigma;
            K1 = k1;
            K2 = k2;
            Theta = theta;
            Nu = nu;
            RhoXY = rhoXy;
            RhoSX = rhoSx;
            RhoSY = rhoSy;
            Asset = asset;
        }
        
        public string Asset { get; private set; }
        public bool WithDivs { get; private set; }
        public MapRawDatas<DateOrDuration, double> Sigma { get; private set; }

        public double K1 { get; private set; }
        public double K2 { get; private set; }
        public double Theta { get; private set; }
        public double Nu { get; private set; }
        public double RhoXY { get; private set; }
        public double RhoSX { get; private set; }
        public double RhoSY { get; private set; }
    }

    internal class Bergomi2FModelFactory : ModelFactory<Bergomi2FModelDescription>
    {
        #region private methods
        private static RrFunction BuildXi(MapRawDatas<DateOrDuration, double> sigma, ITimeMeasure time)
        {
            var matVars = EnumerableUtils.For(0, sigma.Pillars.Length, i =>
            {
                var mat = time[sigma.Pillars[i].ToDate(time.RefDate)];
                var variance = sigma.Values[i] * sigma.Values[i] * mat;
                return new { Mat = mat, Variance = variance };
            }).OrderBy(t => t.Mat).ToArray();

            if (!DoubleUtils.EqualZero(matVars.First().Mat))
            {
                matVars = matVars.Concat(new[] { new { Mat = 0.0, Variance = 0.0 } })
                    .OrderBy(t => t.Mat).ToArray();
            }

            var varianceFunc = RrFunctions.LinearInterpolation(matVars.Map(t => t.Mat),
                                                               matVars.Map(t => t.Variance),
                                                               0.0, double.NaN);
            return varianceFunc.Derivative();
        }
        #endregion
        public static readonly Bergomi2FModelFactory Instance = new Bergomi2FModelFactory();
        public override IModel Build(Bergomi2FModelDescription b2F, Market market)
        {
            ITimeMeasure time = ModelFactoryUtils.DefaultTime(market.RefDate);

            var assetMkt = market.AssetMarketFromName(b2F.Asset);
            var localDividends = b2F.WithDivs
                ? assetMkt.Dividends.Map(div => div.DivModel())
                : new DiscreteLocalDividend[0];

            return new Bergomi2FModel(assetMkt.Asset, localDividends, time, BuildXi(b2F.Sigma, time),
                b2F.K1, b2F.K2, b2F.Theta, b2F.Nu, b2F.RhoXY, b2F.RhoSX, b2F.RhoSY);
        }
    }
    
    public class Bergomi2FModel : EquityModel
    {
        public Bergomi2FModel(AssetId asset, DiscreteLocalDividend[] dividends, ITimeMeasure time, RrFunction xi, 
            double k1, double k2, double theta, double nu, double rhoXy, double rhoSx, double rhoSy)
            : base(asset, dividends, time)
        {
            Xi = xi;
            K1 = k1;
            K2 = k2;
            Theta = theta;
            Nu = nu;
            RhoXY = rhoXy;
            RhoSX = rhoSx;
            RhoSY = rhoSy;
        }

        public RrFunction Xi { get; private set; }
        public double K1 { get; private set; }
        public double K2 { get; private set; }
        public double Theta { get; private set; }
        public double Nu { get; private set; }
        public double RhoXY { get; private set; }
        public double RhoSX { get; private set; }
        public double RhoSY { get; private set; }
    }

    public static class Bergomi2FUtils
    {
        #region private methods
        private static double DoubleExpInt(double k, double t)
        {
            if (Math.Abs(k * k * t * t) > DoubleUtils.MachineEpsilon)
                return (k * t - (1.0 - Math.Exp(-k * t))) / (k * k * t * t);
            return 1.0;
        }
        private static double[,] Correl(Bergomi2FModel b2F)
        {
            return new[,] { { 1.0, b2F.RhoXY }, { b2F.RhoXY, 1.0 } };
        }
        private static double Alpha(Bergomi2FModel b2F)
        {
            var correl = Correl(b2F);
            var thetaVect = new[] { 1.0 - b2F.Theta, b2F.Theta };
            return 1.0 / Math.Sqrt(correl.BilinearProd(thetaVect, thetaVect));
        }
        private static double[][] VarSwapDeformation(Bergomi2FModel b2F, double[] fwdVolStart, double[] fwdVolEnd)
        {
            Debug.Assert(fwdVolStart.Length == fwdVolEnd.Length);

            var initCurve = b2F.Xi.Integral(0.0);
            var factor1 = (b2F.Xi * RrFunctions.Exp(-b2F.K1)).Integral(0.0);
            var factor2 = (b2F.Xi * RrFunctions.Exp(-b2F.K2)).Integral(0.0);
            
            var alpha = Alpha(b2F);
            return EnumerableUtils.For(0, fwdVolStart.Length, i =>
            {
                double volMatStart = fwdVolStart[i];
                double volMatEnd = fwdVolEnd[i];

                double initFwdVariance = initCurve.Eval(volMatEnd) - initCurve.Eval(volMatStart);
                double def1 = factor1.Eval(volMatEnd) - factor1.Eval(volMatStart);
                double def2 = factor2.Eval(volMatEnd) - factor2.Eval(volMatStart);

                return new[] {(1.0 - b2F.Theta) * def1, b2F.Theta * def2}.Mult(b2F.Nu * alpha / initFwdVariance);
            });
        }
        #endregion

        public static double[] FwdVolInstantVol(Bergomi2FModel b2F, double[] fwdVolStart, double[] fwdVolEnd)
        {
            Debug.Assert(fwdVolStart.Length == fwdVolEnd.Length);

            var correl = Correl(b2F);
            var varSwapDeformations = VarSwapDeformation(b2F, fwdVolStart, fwdVolEnd);
            return EnumerableUtils.For(0, fwdVolStart.Length, i =>
            {
                var varSwapDef = varSwapDeformations[i];
                return Math.Sqrt(correl.BilinearProd(varSwapDef, varSwapDef));
            });
        }
        public static double[,] FwdVolInstantCovariance(Bergomi2FModel b2F, double[] fwdVolStart, double[] fwdVolEnd)
        {
            Debug.Assert(fwdVolStart.Length == fwdVolEnd.Length);

            var correl = Correl(b2F);
            var varSwapDeformations = VarSwapDeformation(b2F, fwdVolStart, fwdVolEnd);
            return ArrayUtils.CartesianProd(varSwapDeformations, varSwapDeformations,
                (varSwapDef1, varSwapDef2) => correl.BilinearProd(varSwapDef1, varSwapDef2));
        }
        public static double[] AtmfSkewApprox(Bergomi2FModel b2F, double[] maturities)
        {
            var alpha = Alpha(b2F);
            double coeffX = b2F.Nu * alpha * (1.0 - b2F.Theta) * b2F.RhoSX;
            double coeffY = b2F.Nu * alpha * b2F.Theta * b2F.RhoSY;

            return maturities.Map(t => coeffX * DoubleExpInt(b2F.K1, t) + coeffY * DoubleExpInt(b2F.K2, t));
        }
    }


}
