using System;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.MonteCarlo;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.Model.HullWhite
{
    public class Hw1ModelPathGeneratorFactory : IModelPathGenereratorFactory<Hw1Model>
    {
        #region private methods
        private static RrFunction Drift(Hw1Model model, PaymentInfo probaMeasure)
        {
            if (!model.PivotCurrency.Equals(probaMeasure.Currency))
                throw new Exception("Hw1Model only domestic currency numeraire allowed !");
            
            var drift = model.DriftTerm();

            double probaMat = model.Time[probaMeasure.Date];
            if (probaMat > 0.0)
            {
                var probaDrift = (model.Sigma * model.Sigma) * HwModelUtils.ZcRateCoeffFunction(probaMat, model.MeanReversion);
                drift += probaDrift;
            }

            return drift;
        }
        #endregion
        public IProcessPathGenerator Build(Hw1Model model, PaymentInfo probaMeasure, DateTime[] simulatedDates)
        {
            var dates = model.Time[simulatedDates];
            var drift = Drift(model, probaMeasure);
            var ornsteinUhlenbeck = new OrnsteinUhlenbeck(model.MeanReversion, drift, model.Sigma, 0.0);
            return OrnsteinUhlenbeck1DGeneratorFactory.Build(dates, ornsteinUhlenbeck);
        } 
    }
}