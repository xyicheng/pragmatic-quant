using System;
using System.CodeDom.Compiler;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Microsoft.CSharp;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_model.Product.CouponDsl
{
    public static class DslPayoffCompiler
    {
        #region private fields
        private static int _classIdentifier;
        #endregion
        #region private methods
        private static string GenerateCode(DslPayoffExpression[] expressions, string className, string[] methodIds)
        {
            StringBuilder code = new StringBuilder();
            code.AppendLine("using System;");
            code.AppendLine(String.Format("public class {0}", className));
            code.AppendLine("{");

            for (int i = 0; i < expressions.Length; i++)
            {
                //Payoff Methods
                code.AppendLine(String.Format("     public double {0}(double[] {1})",
                    methodIds[i], expressions[i].FixingArrayId));
                code.AppendLine("   {");
                code.AppendLine("       return " + expressions[i].CSharpExpression + ";");
                code.AppendLine("   }");
            }

            code.AppendLine("}");
            string generatedCode = code.ToString();
            return generatedCode;
        }
        private static Type Compile(string generatedCode, string payoffClassName)
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
                                           .GetType(payoffClassName);
        }
        /// <summary>
        /// Code from Ben Watson's book : "Writing High-Performance .Net Code"
        /// </summary>
        public static T FastMethodCall<T>(MethodInfo methodInfo,
                                          Type extensionType,
                                          Type returnType,
                                          Type[] parametersTypes) where T : class
        {
            var dynamicMethod = new DynamicMethod("Invoke_" + methodInfo.Name, returnType, parametersTypes, true);
            var ilGenerator = dynamicMethod.GetILGenerator();
            
            // object's this parameter
            ilGenerator.Emit(OpCodes.Ldarg_0);
            // cast it to the correct type
            ilGenerator.Emit(OpCodes.Castclass, extensionType);
            // actual method argument
            ilGenerator.Emit(OpCodes.Ldarg_1);
            ilGenerator.EmitCall(OpCodes.Call, methodInfo, null);
            ilGenerator.Emit(OpCodes.Ret);

            object del = dynamicMethod.CreateDelegate(typeof(T));
            return (T)del;
        }
        #endregion

        public static IFixingFunction[] Compile(params DslPayoffExpression[] dslPayoffExpressions)
        {
            string[] methodIds = EnumerableUtils.For(0, dslPayoffExpressions.Length, i => String.Format("Payoff{0}", i));
            string payoffclassName = String.Format("GeneratedDslPayoff{0}", _classIdentifier++);

            string generatedCode = GenerateCode(dslPayoffExpressions, payoffclassName, methodIds);
            Type payoffClassType = Compile(generatedCode, payoffclassName);

            object payoffObj = Activator.CreateInstance(payoffClassType);
            MethodInfo[] payoffMethods = methodIds.Map(methodId => payoffClassType.GetMethod(methodId));

            return EnumerableUtils.For(0, dslPayoffExpressions.Length, i =>
            {
                var fixings = dslPayoffExpressions[i].Fixings.ToArray();
                
                var payoff = FastMethodCall<Func<object, double[], double>>
                    (payoffMethods[i], payoffClassType, typeof (double), new[] {typeof (object), typeof (double[])});

                return new DslPayoffFunction(fixings, payoff, payoffObj) as IFixingFunction;
            });
        }
    }
}