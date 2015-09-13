using System;
using System.Linq;
using System.Diagnostics.Contracts;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Function;
using pragmatic_quant_model.Maths.Interpolation;
using pragmatic_quant_model.Model.Equity;
using pragmatic_quant_model.Model.Equity.LocalVolatility;

namespace pragmatic_quant_model.MarketDatas
{
    public class AssetMarket
    {
        public AssetMarket(AssetId asset, DateTime refDate, ITimeMeasure time,
                           double spot,
                           DiscountCurve riskFreeDiscount,
                           DiscountCurve repoCurve, 
                           DividendQuote[] dividends, 
                           VolatilityMatrix volMatrix)
        {
            Time = time;
            Spot = spot;
            RiskFreeDiscount = riskFreeDiscount;
            RepoCurve = repoCurve;
            Dividends = dividends;
            VolMatrix = volMatrix;
            RefDate = refDate;
            Asset = asset;
            
            if (refDate != repoCurve.RefDate || refDate != time.RefDate)
                throw new Exception("AssetMarket : incompatible ref date !");
        }

        public DateTime RefDate { get; private set; }
        public ITimeMeasure Time { get; private set; }
        public AssetId Asset { get; private set; }
        public double Spot { get; private set; }
        public DividendQuote[] Dividends { get; private set; }
        public DiscountCurve RiskFreeDiscount { get; private set; }
        public DiscountCurve RepoCurve { get; private set; }
        public VolatilityMatrix VolMatrix { get; private set; }

        public DiscountCurve AssetFinancingCurve(DiscountCurve cashFinancingCurve)
        {
            return DiscountCurve.Product(RepoCurve, cashFinancingCurve, FinancingId.AssetCollat(Asset));
        }
        public AssetForwardCurve Forward(DiscountCurve cashFinancingCurve)
        {
            var assetFinancingCurve = AssetFinancingCurve(cashFinancingCurve);
            return new AssetForwardCurve(Spot, Dividends, assetFinancingCurve, Time);
        }
        public AssetForwardCurve Forward()
        {
            return Forward(RiskFreeDiscount);
        }
        public MoneynessProvider Moneyness
        {
            get { return MoneynessProvider.DivAdjusted(Spot, Dividends, RiskFreeDiscount, Time); }
        }
        public VolatilitySurface VolSurface()
        {
            return VolatilitySurface.BuildInterpol(VolMatrix, Moneyness);
        }
    }

    public class DividendQuote
    {
        public DividendQuote(DateTime date, double cash, double yield)
        {
            Date = date;
            Yield = yield;
            Cash = cash;
        }
        
        public DateTime Date { get; private set; }
        public double Cash { get; private set; }
        public double Yield { get; private set; }
    }
    
    //TODO use AffineDivCurveUtils
    public class AssetForwardCurve
    {
        #region private fields
        private readonly ITimeMeasure time;
        private readonly DiscountCurve assetFinancingCurve;

        private readonly StepFunction yieldGrowthFunc;
        private readonly StepFunction cumulatedDividendFunc;
        #endregion
        
        public AssetForwardCurve(double spot, DividendQuote[] dividends, DiscountCurve assetFinancingCurve,
            ITimeMeasure time)
        {
            Contract.Requires(EnumerableUtils.IsSorted(dividends.Select(div => div.Date)));

            Spot = spot;
            this.assetFinancingCurve = assetFinancingCurve;
            this.time = time;

            double[] yieldGrowths = dividends.Scan(1.0, (prev, div) => prev * (1.0 - div.Yield));
            double[] discountedDivCash = dividends.ZipWith(yieldGrowths,
                (div, yGrowth) => div.Cash / yGrowth * assetFinancingCurve.Zc(div.Date));
            double[] cumulatedDividends = discountedDivCash.Scan(0.0, (prev, c) => prev + c);
            var divDates = time[dividends.Select(d => d.Date).ToArray()];

            yieldGrowthFunc = new StepFunction(divDates, yieldGrowths, 1.0);
            cumulatedDividendFunc = new StepFunction(divDates, cumulatedDividends, 0.0);
        }

        public DateTime RefDate
        {
            get { return time.RefDate; }
        }
        public double Spot { get; private set; }
        public double Fwd(DateTime d)
        {
            return (Spot - CumulatedDividends(d)) * AssetGrowth(d);
        }

        public double AssetGrowth(DateTime date)
        {
            return AssetGrowth(time[date]);
        }
        public double AssetGrowth(double d)
        {
            return yieldGrowthFunc.Eval(d) / assetFinancingCurve.Zc(d);
        }

        public double CumulatedDividends(DateTime date)
        {
            return CumulatedDividends(time[date]);
        }
        public double CumulatedDividends(double d)
        {
            return cumulatedDividendFunc.Eval(d);
        } 
    }

    public class VolatilityMatrix
    {
        public VolatilityMatrix(ITimeMeasure time, DateTime[] pillars, double[] strikes, double[,] vols)
        {
            Pillars = pillars;
            Strikes = strikes;
            Vols = vols;
            Time = time;
        }

        public ITimeMeasure Time { get; private set; }
        public DateTime[] Pillars { get; private set; }
        public double[] Strikes { get; private set; }
        public double[,] Vols { get; private set; }
    }

    public class VolatilitySurface
    {
        #region private fields
        
        
        
        #endregion
        protected VolatilitySurface(ITimeMeasure time, MoneynessProvider moneyness, 
                                    VarianceInterpoler varianceInterpoler, LocalVariance localVariance)
        {
            Time = time;
            VarianceInterpoler = varianceInterpoler;
            Moneyness = moneyness;
            LocalVariance = localVariance;
        }

        public static VolatilitySurface BuildInterpol(VolatilityMatrix volMatrix, MoneynessProvider moneyness)
        {
            double[] timePillars = volMatrix.Time[volMatrix.Pillars];
            
            var varianceSlices = EnumerableUtils.For(0, timePillars.Length, i =>
            {
                double t = timePillars[i];
                double[] moneynessPillars = volMatrix.Strikes.Map(k => moneyness.Moneyness(t, k));
                var varianceSlice = volMatrix.Vols.Row(i).Map(v => t * v * v);
                return (RrFunction) SplineInterpoler.BuildCubicSpline(moneynessPillars, varianceSlice);
            });

            var varianceInterpol = new VarianceInterpoler(timePillars, varianceSlices);
            return new VolatilitySurface(volMatrix.Time, moneyness, varianceInterpol, varianceInterpol.BuildLocalVariance());
        }

        public double Variance(double maturity, double strike)
        {
            var m = Moneyness.Moneyness(maturity, strike);
            return VarianceInterpoler.Eval(maturity, m);
        }
        public double Volatility(double maturity, double strike)
        {
            return Math.Sqrt(Variance(maturity, strike) / maturity);
        }
        public double Volatility(DateTime maturity, double strike)
        {
            return Volatility(Time[maturity], strike);
        }
       
        public double LocalVol(double maturity, double strike)
        {
            double y = Moneyness.Moneyness(maturity, strike);
            return Math.Sqrt(LocalVariance.Eval(maturity, y));
        }
        public double LocalVol(DateTime maturity, double strike)
        {
            return LocalVol(Time[maturity], strike);
        }

        public ITimeMeasure Time { get; private set; }
        public MoneynessProvider Moneyness { get; private set; }
        public VarianceInterpoler VarianceInterpoler { get; private set; }
        public LocalVariance LocalVariance { get; private set; }
    }

    public abstract class MoneynessProvider
    {
        public abstract double Moneyness(double maturity, double strike);
        public abstract double Strike(double maturity, double moneyness);

        public static MoneynessProvider FromFwdCurve(AssetForwardCurve assetFwd)
        {
            return new ForwardMoneynessProvider(assetFwd);
        }
        public static MoneynessProvider DivAdjusted(double spot,
                                                    DividendQuote[] dividends,
                                                    DiscountCurve discountCurve,
                                                    ITimeMeasure time)
        {
            return new DivAdjustedMoneyness(spot, new AffineDivCurveUtils(dividends, discountCurve, time));
        }

        #region private class
        private sealed class ForwardMoneynessProvider : MoneynessProvider
        {
            private readonly AssetForwardCurve assetFwd;
            public ForwardMoneynessProvider(AssetForwardCurve assetFwd)
            {
                this.assetFwd = assetFwd;
            }
            public override double Moneyness(double maturity, double strike)
            {
                return Math.Log((strike / assetFwd.AssetGrowth(maturity) + assetFwd.CumulatedDividends(maturity)) / assetFwd.Spot);
            }
            public override double Strike(double maturity, double moneyness)
            {
                return (Math.Exp(moneyness) * assetFwd.Spot - assetFwd.CumulatedDividends(maturity)) * assetFwd.AssetGrowth(maturity);
            }
        }

        private sealed class DivAdjustedMoneyness : MoneynessProvider
        {
            private readonly double spot;
            private readonly AffineDivCurveUtils affineDivCurveUtils;
            public DivAdjustedMoneyness(double spot, AffineDivCurveUtils affineDivCurveUtils)
            {
                this.spot = spot;
                this.affineDivCurveUtils = affineDivCurveUtils;
            }
            public override double Moneyness(double maturity, double strike)
            {
                var c = affineDivCurveUtils.CashDivBpv(maturity);
                var cAverage = affineDivCurveUtils.CashBpvAverage(0.0, maturity);
                var g = affineDivCurveUtils.AssetGrowth(maturity);
                var dk = g * (c - cAverage);

                return Math.Log((strike + dk) / (g * (spot + cAverage)));
            }
            public override double Strike(double maturity, double moneyness)
            {
                throw new NotImplementedException();
            }
        }
        #endregion
    }
    
}