using pragmatic_quant_model.Maths;

namespace pragmatic_quant_model.Model.HullWhite
{
    public class OrnsteinUhlenbeck
    {
        public OrnsteinUhlenbeck(double meanReversion, RrFunction drift, RrFunction volatility)
        {
            Volatility = volatility;
            Drift = drift;
            MeanReversion = meanReversion;
        }
        public double MeanReversion { get; private set; }
        public RrFunction Drift { get; private set; }
        public RrFunction Volatility { get; private set; }
        
    }

    public class OrnsteinUhlenbeckUtils
    {
        public static RrFunction IntegratedDrift(RrFunction instantDrift, double meanReversion, double startDate = 0.0)
        {
            var integratedExpDrift = (instantDrift * RrFunctions.Exp(meanReversion)).Integral(startDate);
            return integratedExpDrift * RrFunctions.Exp(-meanReversion);
        }
        public static RrFunction IntegratedVariance(RrFunction instantVolatility, double meanReversion, double startDate = 0.0)
        {
            return IntegratedCovariance(instantVolatility * instantVolatility, meanReversion, meanReversion, startDate);
        }
        public static RrFunction IntegratedCovariance(RrFunction instantCovariance, double meanReversion1, double meanReversion2, double startDate = 0.0)
        {
            var integratedExpCov = (instantCovariance * RrFunctions.Exp(meanReversion1 + meanReversion2)).Integral(startDate);
            return integratedExpCov * RrFunctions.Exp(-(meanReversion1 + meanReversion2));
        }
    }
}