using System;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.Product;

namespace pragmatic_quant_com.Factories
{
    public class ProductFactory : Singleton<ProductFactory>, IFactoryFromBag<IProduct>
    {
        public IProduct Build(object[,] bag)
        {
            var productName = BagServices.ProcessScalarString(bag, "ProductName")
                                         .ToLowerInvariant().Trim();

            if (productName.EndsWith("leg"))
            {
                var legId = productName.Replace("leg", "");

                var parameters = BagServices.ProcessLabelledMatrix(bag, legId + "PayDate",
                    DateAndDurationConverter.ConvertDate,
                    o => o.ToString(),
                    o => o);

                string couponScript = BagServices.ProcessScalarString(bag, legId + "CouponScript");
                return GenericLegFactory.Build(parameters, couponScript);
            }

            throw new ArgumentException(string.Format("Unknow product {0}", productName));
        }
    }
}