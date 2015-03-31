using System;
using System.Runtime.InteropServices;
using pragmatic_quant_model.Maths;

namespace pragmatic_quant_com
{
    [ComVisible(true)]
    [ClassInterface(ClassInterfaceType.AutoDispatch)]
    [ProgId("PragmaticQuant_VanillaOption")]
    public class VanillaOption
    {
        public object BachelierGreek(double fwd, double strike, double maturity, double vol, string request)
        {
            try
            {
                double gamma, vega, vanna, vomma;
                BachelierOption.Greeks(fwd, strike, maturity, vol,
                    out gamma, out vega, out vanna, out vomma);
                switch (request.Trim().ToLower())
                {
                    case "gamma":
                        return gamma;
                    case "vega":
                        return vega;
                    case "vanna":
                        return vanna;
                    case "vomma":
                        return vomma;
                    default:
                        throw new Exception("Unknow greek : " + request);
                }
            }
            catch (Exception e)
            {
                return "FAILURE: '" + e.Message + "'";
            }
        }
        public object BlackGreek(double fwd, double strike, double maturity, double vol, string request)
        {
            try
            {
                double gamma, vega, vanna, vomma;
                BlackScholesOption.Greeks(fwd, strike, maturity, vol,
                    out gamma, out vega, out vanna, out vomma);
                switch (request.Trim().ToLower())
                {
                    case "gamma":
                        return gamma;
                    case "vega":
                        return vega;
                    case "vanna":
                        return vanna;
                    case "vomma":
                        return vomma;
                    default:
                        throw new Exception("Unknow greek : " + request);
                }
            }
            catch (Exception e)
            {
                return "FAILURE: '" + e.Message + "'";
            }
        }
    }
}
