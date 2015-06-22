using System;
using System.Collections.Generic;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_model.MonteCarlo.Product
{
    public class ProductPathFlowFactory
    {
        #region private fields
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
        private static ArrayPathCalculator<PaymentInfo> NumerairePathCalc(IEnumerable<PaymentInfo> payments,
                                                                           McModel model)
        {
            var simulatedDates = model.SimulatedDates;
            var indexedPaymentsByDate = payments.Select((f, i) => new { Fixing = f, Index = i })
                                              .GroupBy(f => f.Fixing.Date)
                                              .ToArray();
            int[] datesIndexes = indexedPaymentsByDate.Select(g => simulatedDates.FindIndex(g.Key)).ToArray();
            PaymentInfo[][] paymentsByDate = indexedPaymentsByDate.Select(g => g.Select(fi => fi.Fixing).ToArray()).ToArray();
            Func<double[], double>[][] funcsByDate = paymentsByDate.Select(g => g.Select(fi => FlowRebasement(fi, model)).ToArray()).ToArray();
            return new ArrayPathCalculator<PaymentInfo>(datesIndexes, paymentsByDate, funcsByDate);
        }
        private static ArrayPathCalculator<IFixing> FixingPathCalc(DateTime[] simulatedDates,
                                                                   IEnumerable<IFixing> fixings,
                                                                   Func<double[], double>[] fixingFromFactors)
        {
            var indexedFixingsByDate = fixings.Select((f, i) => new { Fixing = f, Index = i })
                                              .GroupBy(f => f.Fixing.Date)
                                              .ToArray();
            int[] datesIndexes = indexedFixingsByDate.Select(g => simulatedDates.FindIndex(g.Key)).ToArray();
            IFixing[][] fixingsByDate = indexedFixingsByDate.Select(g => g.Select(fi => fi.Fixing).ToArray()).ToArray();
            Func<double[], double>[][] funcsByDate = indexedFixingsByDate.Select(g => g.Select(fi => fixingFromFactors[fi.Index]).ToArray()).ToArray();
            return new ArrayPathCalculator<IFixing>(datesIndexes, fixingsByDate, funcsByDate);
        }
        #endregion
        public static ProductPathFlowCalculator Build(IProduct product, McModel model)
        {
            IFixing[] fixings = product.RetrieveFixings();
            Func<double[], double>[] fixingsFunc = fixings.Map(f => model.FactorRepresentation[f]);
            ArrayPathCalculator<IFixing> fixingPathCalc = FixingPathCalc(model.SimulatedDates, fixings, fixingsFunc);

            ArrayPathCalculator<PaymentInfo> numerairePathCalc = NumerairePathCalc(product.RetrievePaymentInfos(), model);

            var pathFlowVisitor = new ProductPathFlowVisitor(fixingPathCalc.Labels, numerairePathCalc.Labels);
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

        public IProductPathFlow Visit(FloatCoupon floatCoupon)
        {
            return BuildCouponPathFlow(simulatedFixings, simulatedRebasement, floatCoupon);
        }
        public IProductPathFlow Visit(FixedCoupon fixedCoupon)
        {
            return BuildCouponPathFlow(simulatedFixings, simulatedRebasement, fixedCoupon);
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
        public CouponArrayPathFlow(Tuple<int, int>[][] fixingsIndexes, Tuple<int, int>[] flowRebasementIndex, Coupon[] coupons)
        {
            this.fixingsIndexes = fixingsIndexes;
            this.flowRebasementIndex = flowRebasementIndex;
            this.coupons = coupons;
            Payments = coupons.Map(cpn => cpn.PaymentInfo);
            Fixings = coupons.Aggregate(new IFixing[0], (prev, cpn) => prev.Union(cpn.Fixings).ToArray())
                .OrderBy(f => f.Date).ToArray();
        }
        
        public PathFlows<double, PaymentInfo> Compute(PathFlows<double[], IFixing[]> fixingsPath,
                                                      PathFlows<double[], PaymentInfo[]> flowRebasementPath)
        {
            double[][] fixings = fixingsPath.Flows;

            var payoffFlows = new double[coupons.Length];
            for (int i = 0; i < coupons.Length; i++)
            {
                var couponFixings = fixingsIndexes[i].Map(index => fixings[index.Item1][index.Item2]);
                var flowRebasement = flowRebasementPath.Flows[flowRebasementIndex[i].Item1]
                                                             [flowRebasementIndex[i].Item2];
                payoffFlows[i] = coupons[i].Payoff(couponFixings) * flowRebasement; 
            }

            return new PathFlows<double, PaymentInfo>(payoffFlows, Payments);
        }
        public IFixing[] Fixings { get; private set; }
        public PaymentInfo[] Payments { get; private set; }
        public int SizeOfPathInBits
        {
            get { return coupons.Length * sizeof (double); }
        }
    }
}