using System;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.Market
{
    public abstract class DiscountProvider
    {
        #region protected fields
        protected readonly DateTime refDate;
        protected readonly ITimeMeasure time;
        #endregion
        #region private fields
        protected DiscountProvider(DateTime refDate)
        {
            this.refDate = refDate;
            time = TimeMeasure.Create(refDate);
        }
        #endregion
        public DateTime RefDate
        {
            get { return refDate; }
        }
        public abstract double Zc(DateTime d);
    }

    public class LinearRateDiscountProvider : DiscountProvider
    {
        #region private fields
        //private readonly 
        #endregion

        public LinearRateDiscountProvider(DateTime[] pillars, double[] zcs, DateTime refDate) 
            : base(refDate)
        {
            if (pillars.Length!=zcs.Length)
                throw new Exception("LinearRateDiscountProvider : Incompatible size");
            
            //var stepSearcher = new StepSearcher()

        }
        public override double Zc(DateTime d)
        {
            throw new NotImplementedException();
        }
    }

}
