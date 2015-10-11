using System;
using System.Collections.Generic;
using pragmatic_quant_model.Basic;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.Product.DslPayoff;
using pragmatic_quant_model.Product.Fixings;

namespace pragmatic_quant_model.Product
{
    using Parameters = LabelledMatrix<DateTime, string, object>;

    public static class DslPayoffFactory
    {
        public static IFixingFunction[] Build(string labelOfRows, Parameters parameters, string[] dslPayoffScripts)
        {
            string[] paramLabels = parameters.ColLabels.Map(s => s.ToLowerInvariant().Trim());
            
            var couponDatas = new DslPayoffExpression[parameters.RowLabels.Length];
            for (int row = 0; row < couponDatas.Length; row++)
            {
                object[] paramVals = parameters.Values.Row(row);
                IDictionary<string, object> couponParameters = paramLabels.ZipToDictionary(paramVals);
                couponParameters.Add(labelOfRows.ToLowerInvariant().Trim(), parameters.RowLabels[row]);

                var payoff = DslPayoffParser.Parse(dslPayoffScripts[row], couponParameters);
                couponDatas[row] = payoff;
            }

            return DslPayoffCompiler.Compile(couponDatas);
        }
    }
}