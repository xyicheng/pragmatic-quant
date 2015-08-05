using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;

namespace pragmatic_quant_model.Product.CouponDsl
{
    public class DslCouponCompiler
    {
        #region private fields
        private const string PayoffClassName = "GeneratedCouponDslPayoff";
        private static int _methodIdentifier; 
        #endregion
        #region private methods
        private static string GetMethodId()
        {
            return string.Format("Payoff{0}", _methodIdentifier++);
        }
        private static MethodInfo CompileToMethod(string couponExpression, string methodId, string fixingArrayId)
        {
            // Generate C# code
            StringBuilder code = new StringBuilder();
            code.AppendLine("using System;");
            code.AppendLine(string.Format("public static partial class {0}", PayoffClassName));
            code.AppendLine("{");

            //PayoffMethod
            code.AppendLine(string.Format("public static double {0}(double[] {1})", methodId, fixingArrayId));
            code.AppendLine("{");
            code.AppendLine("return " + couponExpression + ";");
            code.AppendLine("}");

            code.AppendLine("}");
            string generatedCode = code.ToString();

            // Prepare the compiler.
            CompilerParameters parameters = new CompilerParameters
            {
                GenerateInMemory = true,
                TreatWarningsAsErrors = false,
                GenerateExecutable = false,
                CompilerOptions = "/optimize",
                IncludeDebugInformation = false
            };
            parameters.ReferencedAssemblies.Add("System.dll");

            // Run the compiler.
            CompilerResults results = new CSharpCodeProvider().CompileAssemblyFromSource(parameters, generatedCode);

            if (results.Errors.HasErrors)
            {
                // Something's wrong with the code. Just show the first error.
                throw new Exception("Compile error: " + results.Errors[0]);
            }

            // Locate the method we've just defined (cf. CodeGenerator).
            return results.CompiledAssembly.GetModules()[0]
                .GetType(PayoffClassName)
                .GetMethod(methodId);
        }
        #endregion

        public static DslCoupon BuildCoupon(PaymentInfo paymentInfo, CouponPayoffExpression payoffExpression)
        {
            var payoffMethod = CompileToMethod(payoffExpression.Expression, GetMethodId(), payoffExpression.FixingArrayId);
            return new DslCoupon(paymentInfo, payoffExpression.Fixings.ToArray(), payoffMethod);
        }
    }
}