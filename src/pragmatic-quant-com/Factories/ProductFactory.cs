using System;
using Irony.Parsing;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_com.Factories
{
    public class ProductFactory : Singleton<ProductFactory>, IFactoryFromBag<IProduct>
    {
        #region private methods
        private IProduct BuilAutoCall(IProduct decomposable, object[,] bag)
        {
            var parameters = bag.ProcessLabelledMatrix("AutocallDate",
                                    DateAndDurationConverter.ConvertDate,
                                    o => o.ToString(), o => o);
            object[] redemptionObs;
            if (!parameters.TryGetCol("AutoCallRedemption", out redemptionObs))
                throw new Exception("Missing AutoCallRedemption column");

            string[] redemptionScripts = redemptionObs.Map(o => o.ToString());
            var redemptionPayoffs = GenericLegFactory.BuildDslFixingFunctions(parameters, redemptionScripts);

            throw new NotImplementedException();
        }
        private IProduct BuildCancellable(string cancellableType, ParseTreeNode decomposableNode, object[,] bag)
        {
            IProduct decomposable = BuildProduct(decomposableNode, bag);
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
        private IProduct BuildLeg(string legName, object[,] bag)
        {
            var legId = legName.ToLower().Replace("leg", "");

            var parameters = bag.ProcessLabelledMatrix(legId + "PayDate",
                DateAndDurationConverter.ConvertDate,
                o => o.ToString(), o => o);

            string couponScript = bag.ProcessScalarString(legId + "CouponScript");
            var scripts = EnumerableUtils.ConstantArray(couponScript, parameters.RowLabels.Length);
            Coupon[] coupons = GenericLegFactory.BuildDslCoupons(legId, parameters, scripts);
            return new Leg<Coupon>(coupons);
        }
        private IProduct BuildWeighted(double weight, ParseTreeNode factorProductNode, object[,] bag)
        {
            IProduct factor = BuildProduct(factorProductNode, bag);
            throw new NotImplementedException("Weighted product not yet implemented !");
        }
        private IProduct BuildCombination(ParseTreeNode leftNode, ParseTreeNode rightNode, string op, object[,] bag)
        {
            IProduct left = BuildProduct(leftNode, bag);
            IProduct right = BuildProduct(rightNode, bag);
            switch (op)
            {
                case "+" :
                    throw new NotImplementedException("Combination product not yet implemented !");

                case "-" :
                    throw new NotImplementedException("Combination product not yet implemented !");

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