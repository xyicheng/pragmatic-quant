using System;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.MonteCarlo.Product
{
    public class ProductPathFlowFactory
    {
        #region private fields
        private static IFixing[][] FixingsByDate(IProduct product)
        {
            IFixing[] allFixings = product.RetrieveFixings();
            var fixingsByDate = allFixings.GroupBy(f => f.Date).ToArray();
            return fixingsByDate.Map(g => g.ToArray());
        }
        private static PaymentInfo[][] PaymentsByDate(IProduct product)
        {
            var allPayments = product.RetrievePaymentInfos();
            var paymentsByDate = allPayments.GroupBy(p => p.Date).ToArray();
            return paymentsByDate.Map(g => g.ToArray());
        }
        private static Func<double[], double> FlowRebasement(PaymentInfo payment, McModel model)
        {
            var modelMeasure = model.ProbaMeasure;

            if (modelMeasure.Date < payment.Date
                || !modelMeasure.Currency.Equals(payment.Currency)
                || !modelMeasure.Financing.Equals(payment.Financing))
                throw new NotImplementedException("Flow Rebasement not yet handled !"); //TODO finish the job !

            var zc = new Zc(payment.Date, modelMeasure.Date, payment.Currency, payment.Financing);
            var zcFunc = model.FactorRepresentation[zc];
            double num0 = model.Numeraire0;
            return f => num0/ zcFunc(f);
        }
        private static ArrayPathCalculator<PaymentInfo> NumerairePathCalc(PaymentInfo[][] payments, McModel model)
        {
            var payDates = payments.Map(ps => ps.Map(p => p.Date).Single());
            int[] datesIndexes = payDates.Map(model.SimulatedDates.FindIndex);

            Func<double[], double>[][] funcsByDate = payments.Map(ps => ps.Map(p => FlowRebasement(p, model)));
            return new ArrayPathCalculator<PaymentInfo>(datesIndexes, payments, funcsByDate);
        }
        private static ArrayPathCalculator<IFixing> FixingPathCalc(DateTime[] simulatedDates,
                                                                   IFixing[][] fixings,
                                                                   Func<double[], double>[][] fixingFromFactors)
        {
            var fixingDates = fixings.Map(fs => fs.Map(f => f.Date).Single());
            var dateIndexes = fixingDates.Map(simulatedDates.FindIndex);
            return new ArrayPathCalculator<IFixing>(dateIndexes, fixings, fixingFromFactors);
        }
        #endregion
        public static ProductPathFlowCalculator Build(IProduct product, McModel model)
        {
            IFixing[][] fixings = FixingsByDate(product);
            Func<double[], double>[][] fixingFromFactors = fixings.Map(fs => fs.Map(f => model.FactorRepresentation[f]));
            ArrayPathCalculator<IFixing> fixingPathCalc = FixingPathCalc(model.SimulatedDates, fixings, fixingFromFactors);

            PaymentInfo[][] paymentsByDate = PaymentsByDate(product);
            ArrayPathCalculator<PaymentInfo> numerairePathCalc = NumerairePathCalc(paymentsByDate, model);

            var pathFlowVisitor = new ProductPathFlowVisitor(fixings, paymentsByDate);
            IProductPathFlow productPathFlow = product.Accept(pathFlowVisitor);

            return new ProductPathFlowCalculator(fixingPathCalc, numerairePathCalc, productPathFlow);
        }
    }

    internal class ProductPathFlowVisitor : IProductVisitor<IProductPathFlow>
    {
        #region private fields
        private readonly IFixing[][] simulatedFixings;
        private readonly PaymentInfo[][] simulatedRebasement;
        #endregion
        #region private methods
        private static Tuple<int, int> FindFixingIndex(IFixing[][] fixings, IFixing searchedFixing)
        {
            var dateIndex = fixings.Map(fs => fs.First().Date)
                                   .FindIndex(searchedFixing.Date);
            var fixingIndex = fixings[dateIndex].FindIndex(searchedFixing);
            return new Tuple<int, int>(dateIndex, fixingIndex);
        }
        private static Tuple<int, int> FindPaymentIndex(PaymentInfo[][] payments, PaymentInfo searchedPayment)
        {
            var dateIndex = payments.Map(ps => ps.First().Date)
                                    .FindIndex(searchedPayment.Date);
            var paymentIndex = payments[dateIndex].FindIndex(searchedPayment);
            return new Tuple<int, int>(dateIndex, paymentIndex);
        }
        private static CouponArrayPathFlow BuildCouponPathFlow(IFixing[][] simulatedFixings, PaymentInfo[][] simulatedRebasement, params Coupon[] coupons)
        {
            var fixingIndexes = coupons.Map(cpn => cpn.Fixings.Map(f => FindFixingIndex(simulatedFixings, f)));
            var paymentIndexes = coupons.Map(cpn => FindPaymentIndex(simulatedRebasement, cpn.PaymentInfo));
            return new CouponArrayPathFlow(fixingIndexes, paymentIndexes, coupons);
        }
        #endregion
        public ProductPathFlowVisitor(IFixing[][] simulatedFixings, PaymentInfo[][] simulatedRebasement)
        {
            this.simulatedFixings = simulatedFixings;
            this.simulatedRebasement = simulatedRebasement;
        }

        public IProductPathFlow Visit(Coupon coupon)
        {
            return BuildCouponPathFlow(simulatedFixings, simulatedRebasement, coupon);
        }
        public IProductPathFlow Visit<TCoupon>(Leg<TCoupon> leg) where TCoupon : Coupon
        {
            return BuildCouponPathFlow(simulatedFixings, simulatedRebasement, leg.Coupons.Cast<Coupon>().ToArray());
        }
    }

    internal class CouponArrayPathFlow : IProductPathFlow
    {
        #region private fields
        private readonly Tuple<int, int>[][] fixingsIndexes;
        private readonly Tuple<int, int>[] flowRebasementIndex;
        private readonly Coupon[] coupons;
        #endregion
        #region private fields (buffer)
        private readonly double[][] cpnFixingByDates;
        #endregion
        #region private methods
        private double[][] CouponFixingBuffer()
        {
            var buffer = new double[coupons.Length][];
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = new double[fixingsIndexes[i].Length];
            }
            return buffer;
        }
        #endregion
        public CouponArrayPathFlow(Tuple<int, int>[][] fixingsIndexes, Tuple<int, int>[] flowRebasementIndex, Coupon[] coupons)
        {
            this.fixingsIndexes = fixingsIndexes;
            this.flowRebasementIndex = flowRebasementIndex;
            this.coupons = coupons;
            Payments = coupons.Map(cpn => cpn.PaymentInfo);
            Fixings = coupons.Aggregate(new IFixing[0], (prev, cpn) => prev.Union(cpn.Fixings).ToArray())
                             .OrderBy(f => f.Date)
                             .ToArray();

            //Buffer initialization
            cpnFixingByDates = CouponFixingBuffer();
        }
        
        public void ComputePathFlows(ref PathFlows<double, PaymentInfo> pathFlows, 
                                     PathFlows<double[], IFixing[]> fixingsPath, 
                                     PathFlows<double[], PaymentInfo[]> flowRebasementPath)
        {
            double[][] fixings = fixingsPath.Flows;
            double[][] rebasements = flowRebasementPath.Flows;

            double[] couponsFlows = pathFlows.Flows;
            for (int i = 0; i < coupons.Length; i++)
            {
                Tuple<int, int>[] cpnCoordinates = fixingsIndexes[i];
                double[] couponFixings = cpnFixingByDates[i];
                for (int j = 0; j < cpnCoordinates.Length; j++)
                {
                    Tuple<int, int> coordinate = cpnCoordinates[j];
                    couponFixings[j] = fixings[coordinate.Item1][coordinate.Item2];
                }

                double flowRebasement = rebasements[flowRebasementIndex[i].Item1][flowRebasementIndex[i].Item2];
                couponsFlows[i] = coupons[i].Payoff(couponFixings) * flowRebasement;
            }
        }
        public PathFlows<double, PaymentInfo> NewPathFlow()
        {
            return new PathFlows<double, PaymentInfo>(new double[coupons.Length], Payments);
        }
        public int SizeOfPath
        {
            get { return coupons.Length * sizeof (double); }
        }

        public IFixing[] Fixings { get; private set; }
        public PaymentInfo[] Payments { get; private set; }

    }

}