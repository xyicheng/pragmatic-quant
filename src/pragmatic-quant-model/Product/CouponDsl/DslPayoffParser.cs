using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Irony.Interpreter.Ast;
using Irony.Parsing;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_model.Product.CouponDsl
{
    public static class DslPayoffParser
    {
        #region private fields
        private static readonly Parser grammarParser = InitParser();
        #endregion
        #region private methods
        private static Parser InitParser()
        {
            var payoffLanguage = new LanguageData(new PayoffGrammar());
            return new Parser(payoffLanguage);
        }
        #endregion
        public static DslPayoffExpression Parse(string dslPayoffScript, IDictionary<string, object> couponParameters)
        {
            var payoffParsedTree = grammarParser.Parse(dslPayoffScript);
            var payoffAst = payoffParsedTree.Root.AstNode as AstNode;

            var couponBuilder = new DslExpressionBuilder(couponParameters);
            return couponBuilder.Build(payoffAst);
        }
    }

    internal class DslExpressionBuilder
    {
        #region private fields
        private const string FixingArrayId = "f";
        private readonly IDictionary<string, object> couponParameters;
        #endregion
        #region private methods
        private object RetrieveParameter(string paramName)
        {
            paramName = paramName.Trim().ToLowerInvariant();

            object obj;
            if (!couponParameters.TryGetValue(paramName, out obj))
                throw new Exception(string.Format("Missing coupon parameter : {0} ", paramName));
            return obj;
        }
        private string RetrieveStringParameter(string paramName)
        {
            var obj = RetrieveParameter(paramName);
            return obj.ToString();
        }
        private DateTime RetrieveDateParameter(string paramName)
        {
            var obj = RetrieveParameter(paramName);
            return DateAndDurationConverter.ConvertDate(obj);
        }
        
        private string FixingReference(IFixing fixing, IDictionary<IFixing, string> fixingRefs)
        {
            string fixingRef;
            if (!fixingRefs.TryGetValue(fixing, out fixingRef))
            {
                fixingRef = string.Format("{0}[{1}]", FixingArrayId, fixingRefs.Count);
                fixingRefs.Add(fixing, fixingRef);
            }
            return fixingRef;
        }
        private string DoubleReference(double number)
        {
            return number.ToString("r", CultureInfo.InvariantCulture) + "d";
        }
        private string ObjectReference(object o)
        {
            if (o is double)
                return DoubleReference((double)o);
            
            if (o is int)
                return DoubleReference((int)o);

            return o.ToString();
        }

        private string GetExpression(FixingNode fixingNode, IDictionary<IFixing, string> fixingRefs)
        {
            string fixingDesc = RetrieveStringParameter(fixingNode.FixingId);
            DateTime fixingDate = RetrieveDateParameter(fixingNode.ScheduleId);
            IFixing fixing = FixingParser.Parse(fixingDesc, fixingDate);
            return FixingReference(fixing, fixingRefs);
        }
        private void CheckArgumentSize(string func, string[] args, int expectedSize)
        {
            if (args.Length != expectedSize)
                throw new ArgumentException(string.Format("{0} expect {1} arguments, but was {2}.",
                                                           func, expectedSize, args.Length));
        }
        private string FunctionReference(string target, string[] args)
        {
            switch (target.Trim().ToLower())
            {
                case "max":
                    CheckArgumentSize("Max", args, 2);
                    return "System.Math.Max";
                case "min":
                    CheckArgumentSize("Min", args, 2);
                    return "System.Math.Min";
            }
            throw new ArgumentException(string.Format("Unknown function {0}", target));
        }
        private string GetExpression(FunctionCallNode funcNode, IDictionary<IFixing, string> fixingRefs)
        {
            var argNodes = funcNode.Arguments.ChildNodes;
            var argumentExpressions = argNodes.Map(node => GetExpression(node, fixingRefs));
            var args = argumentExpressions.Skip(1).Fold(argumentExpressions[0], (expr, arg) => expr + ", " + arg);
            var funcRef = FunctionReference(funcNode.TargetName, argumentExpressions);
            return funcRef + "(" + args + ")";
        }
        private string GetExpression(LiteralValueNode litValueNode, IDictionary<IFixing, string> fixingRefs)
        {
            var value = litValueNode.Value;
            return ObjectReference(value);
        }
        private string GetExpression(BinaryOperationNode binOp, IDictionary<IFixing, string> fixingRefs)
        {
            var leftExpression = GetExpression(binOp.Left, fixingRefs);
            var rightExpression = GetExpression(binOp.Right, fixingRefs);
            return "(" + leftExpression + ")" + binOp.OpSymbol + "(" + rightExpression + ")";
        }
        private string GetExpression(IdentifierNode id, IDictionary<IFixing, string> fixingRefs)
        {
            var idValue = RetrieveParameter(id.Symbol);
            return ObjectReference(idValue);
        }
        private string GetExpression(AstNode node, IDictionary<IFixing, string> fixingRefs)
        {
            var fixing = node as FixingNode;
            if (fixing != null)
                return GetExpression(fixing, fixingRefs);

            var litVal = node as LiteralValueNode;
            if (litVal != null)
                return GetExpression(litVal, fixingRefs);

            var binOp = node as BinaryOperationNode;
            if (binOp != null)
                return GetExpression(binOp, fixingRefs);

            var function = node as FunctionCallNode;
            if (function != null)
                return GetExpression(function, fixingRefs);

            var identifier = node as IdentifierNode;
            if (identifier != null)
                return GetExpression(identifier, fixingRefs); 

            throw new ArgumentException(string.Format("Not handled node {0} ", node));
        }
        #endregion
        public DslExpressionBuilder(IDictionary<string, object> couponParameters)
        {
            this.couponParameters = couponParameters;
        }

        public DslPayoffExpression Build(AstNode node)
        {
            IDictionary<IFixing, string> fixingRefs = new Dictionary<IFixing, string>();
            var expression = GetExpression(node, fixingRefs);
            return new DslPayoffExpression(fixingRefs.Keys.ToArray(), expression, FixingArrayId);
        }
    }
}