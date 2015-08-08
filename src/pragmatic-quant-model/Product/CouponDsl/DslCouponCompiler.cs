using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CSharp;
using pragmatic_quant_model.Basic;

namespace pragmatic_quant_model.Product.CouponDsl
{
    public static class DslCouponCompiler
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
        private static string GenerateCode(DslPayoffExpression[] expressions, string[] methodIds)
        {
            StringBuilder code = new StringBuilder();
            code.AppendLine("using System;");
            code.AppendLine(string.Format("public static partial class {0}", PayoffClassName));
            code.AppendLine("{");

            for (int i = 0; i < expressions.Length; i++)
            {
                //Payoff Methods
                code.AppendLine(string.Format("public static double {0}(double[] {1})",
                    methodIds[i], expressions[i].FixingArrayId));
                code.AppendLine("{");
                code.AppendLine("return " + expressions[i].Expression + ";");
                code.AppendLine("}");
            }

            code.AppendLine("}");
            string generatedCode = code.ToString();
            return generatedCode;
        }
        private static Type Compile(string generatedCode)
        {
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
                                           .GetType(PayoffClassName);
        }
        #endregion

        public static DslCoupon[] Compile(params DslCouponData[] dslCouponDatas)
        {
            var methodIds = EnumerableUtils.For(0, dslCouponDatas.Length, i => GetMethodId());
            string generatedCode = GenerateCode(dslCouponDatas.Map(d => d.Expression), methodIds);
            Type methodClassContainer = Compile(generatedCode);
            MethodInfo[] payoffMethods = methodIds.Map(methodId => methodClassContainer.GetMethod(methodId));

            return EnumerableUtils.For(0, dslCouponDatas.Length, i =>
            {
                var fixings = dslCouponDatas[i].Expression.Fixings.ToArray();
                var paymentInfo = dslCouponDatas[i].PaymentInfo;
                return new DslCoupon(paymentInfo, fixings, payoffMethods[i]);
            });
        }
    }
}