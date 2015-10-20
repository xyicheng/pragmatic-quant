using System;
using Irony.Parsing;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Product;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_com.Factories
{
    using Parameters = LabelledMatrix<DateTime, string, object>;

    public class ProductFactory : Singleton<ProductFactory>, IFactoryFromBag<IProduct>
    {
        #region private methods
        private static Coupon[] BuildDslCoupons(string legId, Parameters parameters, string[] dslCouponPayoffs)
        {
            var payCurrencies = parameters.GetColFromLabel(legId + "PayCurrency", o => Currency.Parse(o.ToString()));
            var payDates = parameters.RowLabels;
            var couponPayments = payDates.ZipWith(payCurrencies, (d, c) => new PaymentInfo(c, d));

            var couponPayoffs = DslPayoffFactory.Build(legId +"Date", parameters, dslCouponPayoffs);
            return couponPayments.ZipWith(couponPayoffs, (payInfo, payoff) => new Coupon(payInfo, payoff));
        }

        private IProduct BuilAutoCall(ICouponDecomposable underlying, object[,] bag)
        {
            var parameters = bag.ProcessLabelledMatrix("AutocallDate",
                DateAndDurationConverter.ConvertDate,
                o => o.ToString(), o => o);

            var redemptionScripts = parameters.GetColFromLabel("Redemption", o => o.ToString());
            var currencies = parameters.GetColFromLabel("RedemptionCurrency", o => Currency.Parse(o.ToString()));
            var payDates = parameters.GetColFromLabel("RedemptionDate", DateAndDurationConverter.ConvertDate);
            
            PaymentInfo[] payInfos = payDates.ZipWith(currencies, (d, c) => new PaymentInfo(c, d));
            IFixingFunction[] redemptionPayoffs = DslPayoffFactory.Build("AutocallDate", parameters, redemptionScripts);
            var redemptionCoupons = redemptionPayoffs.ZipWith(payInfos, (payoff, payInfo) => new Coupon(payInfo, payoff));

            var triggerScripts = parameters.GetColFromLabel("AutocallTrigger", o => o.ToString());
            var triggers = DslPayoffFactory.Build("AutocallDate", parameters, triggerScripts);

            return new AutoCall(underlying, parameters.RowLabels, redemptionCoupons, triggers);
        }
        private IProduct BuildCancellable(string cancellableType, ParseTreeNode decomposableNode, object[,] bag)
        {
            var decomposable = (ICouponDecomposable) BuildProduct(decomposableNode, bag);
            switch (cancellableType.ToLowerInvariant())
            {
                case "autocall" :
                    return BuilAutoCall(decomposable, bag);

                case "callable" :

                case "target" :
                
                default :
                    throw new Exception(string.Format("Error while parsing product, unknown cancellable type : {0}", cancellableType));
            }
        }
        private Leg<Coupon> BuildLeg(string legName, object[,] bag)
        {
            var legId = legName.ToLower().Replace("leg", "");

            var parameters = bag.ProcessLabelledMatrix(legId + "PayDate",
                DateAndDurationConverter.ConvertDate,
                o => o.ToString(), o => o);

            string couponScript = bag.ProcessScalarString(legId + "CouponScript");
            var scripts = EnumerableUtils.ConstantArray(couponScript, parameters.RowLabels.Length);
            Coupon[] coupons = BuildDslCoupons(legId, parameters, scripts);
            return new Leg<Coupon>(coupons);
        }
        private IProduct BuildWeighted(double weight, ParseTreeNode factorProductNode, object[,] bag)
        {
            var factor = BuildProduct(factorProductNode, bag) as ICouponDecomposable;
            return DecomposableLinearCombination.Create(new[] {weight}, new[] {factor});
        }
        private IProduct BuildCombination(ParseTreeNode leftNode, ParseTreeNode rightNode, string op, object[,] bag)
        {
            ICouponDecomposable left = BuildProduct(leftNode, bag) as ICouponDecomposable;
            ICouponDecomposable right = BuildProduct(rightNode, bag) as ICouponDecomposable;
            switch (op)
            {
                case "+" :
                    return DecomposableLinearCombination.Create(new[] {1.0, 1.0}, new[] {left, right});

                case "-" :
                    return DecomposableLinearCombination.Create(new[] {1.0, -1.0}, new[] {left, right});

                default :
                    throw new Exception("BUG should never get there !");
            }
        }
        private IProduct BuildProduct(ParseTreeNode productNode, object[,] bag)
        {
            switch (productNode.Term.Name)
            {
                case "Cancellable":

                    ParseTreeNode cancellableTypeNode = productNode.ChildNodes[0];
                    string cancellableType = cancellableTypeNode.ChildNodes[0].Token.ValueString;
                    ParseTreeNode decomposableNode = productNode.ChildNodes[2];
                    return BuildCancellable(cancellableType, decomposableNode, bag);

                case "Leg" :
                    return BuildLeg(productNode.Token.ValueString, bag);

                case "Coupon" :
                    throw new NotImplementedException("Single Coupon product not yet implemented !");
                
                case "ParDecomposable" :
                    return BuildProduct(productNode.ChildNodes[1], bag);

                case "WeightedDecomposable" :
                    double weight = double.Parse(productNode.ChildNodes[0].Token.ValueString);
                    return BuildWeighted(weight, productNode.ChildNodes[2], bag);
                
                case "DecomposableCombination" :
                    var left = productNode.ChildNodes[0];
                    var right = productNode.ChildNodes[2];
                    var op = productNode.ChildNodes[1].Token.ValueString;
                    return BuildCombination(left, right, op, bag);

                default :
                    throw new Exception("BUG, Should never get there !");
            }
        }
        #endregion
        
        public IProduct Build(object[,] bag)
        {
            string product = bag.ProcessScalarString("ProductName");
            ParseTree productTree = new Parser(new ProductGrammar()).Parse(product);
            return BuildProduct(productTree.Root, bag);
        }
    }

    [Language("Product DSL", "1.0", "Financial product description")]
    internal class ProductGrammar : Grammar
    {
        public ProductGrammar()
            : base(false)
        {
            #region 1. Terminals
            var number = new NumberLiteral("number", NumberOptions.AllowSign);
            var leg = new RegexBasedTerminal("Leg", @"[a-z]+leg");
            var coupon = new RegexBasedTerminal("Coupon", @"[a-z]+coupon");
            #endregion

            #region 2. Non-terminals
            var Product = new NonTerminal("Product");
            var Cancellable = new NonTerminal("Cancellable");
            var CancellableType = new NonTerminal("CancellableType");
            var DecomposableProduct = new NonTerminal("DecomposableProduct");
            var Term = new NonTerminal("Term");
            var WeightedDecomposable = new NonTerminal("WeightedDecomposable");
            var DecomposableCombination = new NonTerminal("DecomposableCombination");
            var CombinationOp = new NonTerminal("CombinationOp", "operator");
            var ParDecomposable = new NonTerminal("ParDecomposable");
            #endregion

            #region 3. BNF rules
            Product.Rule = DecomposableProduct | Cancellable;

            Cancellable.Rule = CancellableType + PreferShiftHere() + "[" + DecomposableProduct + "]";
            CancellableType.Rule = ToTerm("AutoCall") | "Callable" | "Target";

            DecomposableProduct.Rule = Term | WeightedDecomposable | DecomposableCombination;
            Term.Rule = ParDecomposable | coupon | leg;
            WeightedDecomposable.Rule = number + "*" + Term;
            ParDecomposable.Rule = "(" + DecomposableProduct + ")";
            DecomposableCombination.Rule = DecomposableProduct + CombinationOp + DecomposableProduct;
            CombinationOp.Rule = ToTerm("+") | "-";
            #endregion

            Root = Product;

            #region 4. Operators precedence
            RegisterOperators(10, "?");
            RegisterOperators(15, "&", "&&", "|", "||");
            RegisterOperators(20, "==", "<", "<=", ">", ">=", "!=");
            RegisterOperators(30, "+", "-");
            RegisterOperators(40, "*", "/");
            RegisterOperators(50, Associativity.Right, "**");
            // For precedence to work, we need to take care of one more thing: BinOp. 
            //For BinOp which is or-combination of binary operators, we need to either 
            // 1) mark it transient or 2) set flag TermFlags.InheritPrecedence
            // We use first option, making it Transient.  

            // 5. Punctuation and transient terms
            MarkPunctuation("(", ")");
            RegisterBracePair("(", ")");
            RegisterBracePair("[", "]");
            MarkTransient(Term, DecomposableProduct, CombinationOp, ParDecomposable, Product);
            #endregion
        }
    }
    
}