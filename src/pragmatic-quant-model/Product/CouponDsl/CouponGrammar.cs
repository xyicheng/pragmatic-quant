using System;
using Irony.Ast;
using Irony.Interpreter;
using Irony.Interpreter.Ast;
using Irony.Parsing;

namespace pragmatic_quant_model.Product.CouponDsl
{
    [Language("Coupon Payoff DSL", "1.0", "Financial coupon description")]
    public class CouponGrammar : InterpretedLanguageGrammar
    {
        public CouponGrammar()
            : base(false)
        {
            #region 1. Terminals
            var number = new NumberLiteral("number", NumberOptions.NoDotAfterInt)
            {
                //Let's allow big integers (with unlimited number of digits):
                DefaultIntTypes = new [] { TypeCode.Int32, TypeCode.Int64, NumberLiteral.TypeCodeBigInt }
            };
            var comma = ToTerm(",");
            var identifier = new IdentifierTerminal("identifier");
            
            var scheduleId = new RegexBasedTerminal("scheduleId", @"[a-z]+date");
            scheduleId.AstConfig.NodeType = typeof(ScheduleIdNode);
            
            //String literal with embedded expressions  ------------------------------------------------------------------
            var stringLit = new StringLiteral("string", "\"", StringOptions.AllowsAllEscapes | StringOptions.IsTemplate);
            stringLit.AddStartEnd("'", StringOptions.AllowsAllEscapes | StringOptions.IsTemplate);
            stringLit.AstConfig.NodeType = typeof(StringTemplateNode);
            var Expr = new NonTerminal("Expr"); //declare it here to use in template definition 
            var templateSettings = new StringTemplateSettings //by default set to Ruby-style settings 
            {
                //this defines how to evaluate expressions inside template
                ExpressionRoot = Expr
            };
            this.SnippetRoots.Add(Expr);
            stringLit.AstConfig.Data = templateSettings;
            //--------------------------------------------------------------------------------------------------------
            #endregion
            
            #region 2. Non-terminals
            var Term = new NonTerminal("Term");
            var BinExpr = new NonTerminal("BinExpr", typeof(BinaryOperationNode));
            var ParExpr = new NonTerminal("ParExpr");
            var UnExpr = new NonTerminal("UnExpr", typeof(UnaryOperationNode));
            var TernaryIfExpr = new NonTerminal("TernaryIf", typeof(IfNode));
            var ArgList = new NonTerminal("ArgList", typeof(ExpressionListNode));
            var FunctionCall = new NonTerminal("FunctionCall", typeof(FunctionCallNode));
            var MemberAccess = new NonTerminal("MemberAccess", typeof(MemberAccessNode));

            var UnOp = new NonTerminal("UnOp");
            var BinOp = new NonTerminal("BinOp", "operator");

            var Fixing = new NonTerminal("Fixing", typeof(FixingNode));
            #endregion
            
            #region 3. BNF rules
            Expr.Rule = Term | UnExpr | BinExpr | TernaryIfExpr | Fixing;
            Term.Rule = number | ParExpr | stringLit | FunctionCall | identifier | MemberAccess;
            ParExpr.Rule = "(" + Expr + ")";
            UnExpr.Rule = UnOp + Term + ReduceHere();
            UnOp.Rule = ToTerm("+") | "-" | "!";
            BinExpr.Rule = Expr + BinOp + Expr;
            BinOp.Rule = ToTerm("+") | "-" | "*" | "/" | "**" | "==" | "<" | "<=" | ">" | ">=" | "!=" | "&&" | "||" | "&" | "|";

            TernaryIfExpr.Rule = Expr + "?" + Expr + ":" + Expr;
            MemberAccess.Rule = Expr + PreferShiftHere() + "." + identifier;
            ArgList.Rule = MakeStarRule(ArgList, comma, Expr);
            FunctionCall.Rule = identifier + PreferShiftHere() + "(" + ArgList + ")";
            FunctionCall.NodeCaptionTemplate = "call #{0}(...)";

            Fixing.Rule = (identifier ) + PreferShiftHere() + "@" + scheduleId;
            #endregion
            
            Root = Expr;       // Set grammar root

            // 4. Operators precedence
            RegisterOperators(10, "?");
            RegisterOperators(15, "&", "&&", "|", "||");
            RegisterOperators(20, "==", "<", "<=", ">", ">=", "!=");
            RegisterOperators(30, "+", "-");
            RegisterOperators(40, "*", "/");
            RegisterOperators(50, Associativity.Right, "**");
            RegisterOperators(60, "!");
            // For precedence to work, we need to take care of one more thing: BinOp. 
            //For BinOp which is or-combination of binary operators, we need to either 
            // 1) mark it transient or 2) set flag TermFlags.InheritPrecedence
            // We use first option, making it Transient.  

            // 5. Punctuation and transient terms
            MarkPunctuation("(", ")", "?", ":", "[", "]");
            RegisterBracePair("(", ")");
            RegisterBracePair("[", "]");
            MarkTransient(Term, Expr, BinOp, UnOp, ParExpr);

            // 7. Syntax error reporting
            MarkNotReported("++", "--");
            AddToNoReportGroup("(", "++", "--");
            AddToNoReportGroup(NewLine);
            AddOperatorReportGroup("operator");
            AddTermsReportGroup("assignment operator", "=", "+=", "-=", "*=", "/=");

            //8. Console
            ConsoleTitle = "Irony Expression Evaluator";
            ConsoleGreeting =
      @"Irony Expression Evaluator 

  Supports variable assignments, arithmetic operators (+, -, *, /),
    augmented assignments (+=, -=, etc), prefix/postfix operators ++,--, string operations. 
  Supports big integer arithmetics, string operations.
  Supports strings with embedded expressions : ""name: #{name}""

Press Ctrl-C to exit the program at any time.
";
            ConsolePrompt = "?";
            ConsolePromptMoreInput = "?";

            //9. Language flags. 
            // Automatically add NewLine before EOF so that our BNF rules work correctly when there's no final line break in source
            LanguageFlags = LanguageFlags.NewLineBeforeEOF | LanguageFlags.CreateAst | LanguageFlags.SupportsBigInt;
        }
    }

    public class ScheduleIdNode : AstNode
    {
        public string ScheduleName;
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            ScheduleName = treeNode.FindTokenAndGetText();
            AsString = ScheduleName;
        }
    }

    public class FixingNode : AstNode
    {
        public string FixingId;
        public string ScheduleId;

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            FixingId = nodes[0].FindTokenAndGetText();
            ScheduleId = nodes[2].FindTokenAndGetText();

            AsString = "Fixing ";
        }
    }

    public class FunctionCallNode : AstNode
    {
        #region private fields
        private AstNode targetRef;
        #endregion

        public ExpressionListNode Arguments;
        public string TargetName;

        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            targetRef = AddChild("Target", nodes[0]);
            targetRef.UseType = NodeUseType.CallTarget;
            TargetName = nodes[0].FindTokenAndGetText();
            Arguments = AddChild("Args", nodes[1]) as ExpressionListNode;
            AsString = "Call " + TargetName;
        }
    }

}
