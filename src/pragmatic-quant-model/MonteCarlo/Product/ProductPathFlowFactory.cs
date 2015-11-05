using System;
using System.Collections.Generic;
using System.Linq;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Product;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_model.MonteCarlo.Product
{
    public class ProductPathFlowFactory
    {
        #region private methods
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
        private static Tuple<int, int> FindPaymentIndex(PaymentInfo[][] payments, PaymentInfo searchedPayment)
        {
            var dateIndex = payments.Map(ps => ps.First().Date)
                                    .FindIndex(searchedPayment.Date);
            var paymentIndex = payments[dateIndex].FindIndex(searchedPayment);
            return new Tuple<int, int>(dateIndex, paymentIndex);
        }
        private static Tuple<int, int> FindFixingIndex(IFixing[][] fixings, IFixing searchedFixing)
        {
            var dateIndex = fixings.Map(fs => fs.First().Date)
                                   .FindIndex(searchedFixing.Date);
            var fixingIndex = fixings[dateIndex].FindIndex(searchedFixing);
            return new Tuple<int, int>(dateIndex, fixingIndex);
        }
        private FixingFuncPathValue FixingFuncPathValue(IFixingFunction payoff)
        {
            var coords = payoff.Fixings.Map(f => FindFixingIndex(simulatedFixings, f));
            return new FixingFuncPathValue(payoff, coords);
        }
        private CouponPathFlow[] BuildCouponPathFlow(params Coupon[] coupons)
        {
            CouponPathFlow[] couponFlows = coupons.Map(cpn =>
            {
                var payoffPathValue = FixingFuncPathValue(cpn.Payoff);
                var paymentCoordinate = FindPaymentIndex(simulatedRebasement, cpn.PaymentInfo);
                var couponFlowLabel = new CouponFlowLabel(cpn.PaymentInfo, cpn.ToString()); 
                return new CouponPathFlow(payoffPathValue, couponFlowLabel, paymentCoordinate);
            });
            return couponFlows;
        }
        #endregion
        public ProductPathFlowVisitor(IFixing[][] simulatedFixings, PaymentInfo[][] simulatedRebasement)
        {
            this.simulatedFixings = simulatedFixings;
            this.simulatedRebasement = simulatedRebasement;
        }

        public IProductPathFlow Visit(Coupon coupon)
        {
            var couponFlows = BuildCouponPathFlow(coupon);
            return new CouponArrayPathFlow(couponFlows);
        }
        public IProductPathFlow Visit(ICouponDecomposable couponDecomposable)
        {
            var couponFlows = BuildCouponPathFlow(couponDecomposable.Decomposition());
            return new CouponArrayPathFlow(couponFlows); 
        }
        public IProductPathFlow Visit(AutoCall autocall)
        {
            List<Coupon> underlyingsCouponsByPayDate = autocall.Underlying.Decomposition()
                                                            .OrderBy(cpn => cpn.PaymentInfo.Date)
                                                            .ToList();
            CouponPathFlow[] underlyingPathFlows = BuildCouponPathFlow(underlyingsCouponsByPayDate.ToArray());

            int[] underlyingCallIndexes = autocall.CallDates.Map(callDate =>
            {
                var idx = underlyingsCouponsByPayDate.FindIndex(cpn => cpn.PaymentInfo.Date > callDate);
                if (idx == -1)
                    return underlyingsCouponsByPayDate.Count;
                return idx;
            });
            
            var triggerPathEvals = autocall.CallDates.Map(callDate =>
            {
                var trigger = autocall.CallTrigger(callDate);
                return FixingFuncPathValue(trigger);
            });

            var redemptionPathFlows = autocall.CallDates.Map(callDate =>
            {
                var redemptionCpn = autocall.Redemption(callDate);
                return BuildCouponPathFlow(redemptionCpn).Single();
            });

            return new AutocallPathFlow(underlyingPathFlows, redemptionPathFlows, triggerPathEvals, underlyingCallIndexes);
        }
    }

}