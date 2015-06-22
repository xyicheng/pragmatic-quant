using System;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Maths;

namespace pragmatic_quant_model.Model.BlackScholes
{
    public class BlackScholesModel : IModel
    {
        public BlackScholesModel(ITimeMeasure time, AssetId asset, RrFunction sigma, DiscreteLocalDividend[] dividends)
        {
            Dividends = dividends;
            Sigma = sigma;
            Asset = asset;
            Time = time;
        }

        public AssetId Asset { get; private set; }
        public RrFunction Sigma { get; private set; }
        public DiscreteLocalDividend[] Dividends { get; private set; }

        public ITimeMeasure Time { get; private set; }
        public Currency PivotCurrency { get { return Asset.Currency; } }
    }

    public abstract class DiscreteLocalDividend
    {
        #region protected method
        protected DiscreteLocalDividend(DateTime date)
        {
            Date = date;
        }
        #endregion

        public static DiscreteLocalDividend AffineDividend(DateTime date, double cash, double yield)
        {
            return new AffineLocalDividend(date, cash, yield);
        }
        public static DiscreteLocalDividend ZeroDiv(DateTime date)
        {
            return AffineDividend(date, 0.0, 0.0);
        }

        public abstract double Value(double spotValue);
        public DateTime Date { get; private set; }
        
        #region private class
        private class AffineLocalDividend : DiscreteLocalDividend
        {
            private readonly double cash;
            private readonly double yield;
            public AffineLocalDividend(DateTime date, double cash, double yield)
                : base(date)
            {
                this.cash = cash;
                this.yield = yield;
            }
            public override double Value(double spotValue)
            {
                return yield * spotValue + cash;
            }
        }
        #endregion
    }
}
