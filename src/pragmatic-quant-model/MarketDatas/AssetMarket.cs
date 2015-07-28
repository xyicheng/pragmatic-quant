using System;
using System.Diagnostics.Contracts;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Function;
using pragmatic_quant_model.Maths.Interpolation;

namespace pragmatic_quant_model.MarketDatas
{
    public class AssetMarket
    {
        #region private fields
        private readonly ITimeMeasure time;
        private readonly double spot;
        private readonly DiscountCurve repoCurve;
        private readonly DividendQuote[] dividends;
        #endregion
        
        public AssetMarket(AssetId asset, DateTime refDate, ITimeMeasure time,
            double spot, DiscountCurve repoCurve, DividendQuote[] dividends, VolatilityMatrix volMatrix)
        {
            this.time = time;
            this.spot = spot;
            this.repoCurve = repoCurve;
            this.dividends = dividends;
            VolMatrix = volMatrix;
            RefDate = refDate;
            Asset = asset;
            
            if (refDate != repoCurve.RefDate || refDate != time.RefDate)
                throw new Exception("AssetMarket : incompatible ref date !");
        }

        public DateTime RefDate { get; private set; }
        public AssetId Asset { get; private set; }
        public double Spot
        {
            get { return spot; }
        }
        public DividendQuote[] Dividends
        {
            get { return dividends; }
        }
        public DiscountCurve RepoCurve
        {
            get { return repoCurve; }
        }
        public VolatilityMatrix VolMatrix { get; private set; }

        public DiscountCurve AssetFinancingCurve(DiscountCurve cashFinancingCurve)
        {
            return DiscountCurve.Product(repoCurve, cashFinancingCurve, FinancingId.AssetCollat(Asset));
        }
        public AssetForwardCurve Forward(DiscountCurve cashFinancingCurve)
        {
            var assetFinancingCurve = AssetFinancingCurve(cashFinancingCurve);
            return new AssetForwardCurve(spot, dividends, assetFinancingCurve, time);
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
        private readonly MixedLinearInterpolation2D varianceInterpoler;
        private readonly MoneynessProvider moneyness; 
        #endregion
        protected VolatilitySurface(ITimeMeasure time, MixedLinearInterpolation2D varianceInterpoler, MoneynessProvider moneyness)
        {
            Time = time;
            this.varianceInterpoler = varianceInterpoler;
            this.moneyness = moneyness;
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

            var varianceInterpol = new MixedLinearInterpolation2D(timePillars, varianceSlices, varianceSlices.First());
            return new VolatilitySurface(volMatrix.Time, varianceInterpol, moneyness);
        }

        public double Variance(double maturity, double strike)
        {
            var m = moneyness.Moneyness(maturity, strike);
            return varianceInterpoler.Eval(maturity, m);
        }
        public double Volatility(double maturity, double strike)
        {
            return Math.Sqrt(Variance(maturity, strike) / maturity);
        }
        public double Volatility(DateTime maturity, double strike)
        {
            return Volatility(Time[maturity], strike);
        }
        public ITimeMeasure Time { get; private set; }
    }

    public abstract class MoneynessProvider
    {
        public abstract double Moneyness(double maturity, double strike);
        public abstract double Strike(double maturity, double moneyness);

        public static MoneynessProvider FromFwdCurve(AssetForwardCurve assetFwd)
        {
            return new ForwardMoneynessProvider(assetFwd);
        }

        #region private class
        private class ForwardMoneynessProvider : MoneynessProvider
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
        #endregion
    }
    
}