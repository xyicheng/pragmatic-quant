using System;
using ExcelDna.Integration;
using pragmatic_quant_model.Maths;
using Bachelier = pragmatic_quant_model.Maths.BachelierOption;

namespace pragmatic_quant_com
{
    public class VanillaOption
    {
        [ExcelFunction(Description = "Compute vanilla option price within Bachelier model", 
                       Category = "PragmaticQuant_VanillaOption")]
        public static object BachelierOption(double forward, double strike, double maturity, double vol, string optionType)
        {
            return FunctionRunnerUtils.Run("BachelierOption", () =>
            {
                double q;
                switch (optionType.Trim().ToLower())
                {
                    case "call":
                        q = 1.0;
                        break;
                    case "put":
                        q = -1.0;
                        break;
                    default:
                        throw new Exception(string.Format("Unknow option type : {0}", optionType));
                }
                return Bachelier.Price(forward, strike, vol, maturity, q);
            });
        }

        [ExcelFunction(Description = "Compute vanilla option implied vol within Bachelier model", 
                       Category = "PragmaticQuant_VanillaOption")]
        public static object BachelierImpliedVol(double forward, double strike, double maturity, double price, string optionType)
        {
            return FunctionRunnerUtils.Run("BachelierImpliedVol", () =>
            {
                double q;
                switch (optionType.Trim().ToLower())
                {
                    case "call":
                        q = 1.0;
                        break;
                    case "put":
                        q = -1.0;
                        break;
                    default:
                        throw new Exception(string.Format("Unknow option type : {0}", optionType));
                }
                return Bachelier.ImpliedVol(price, forward, strike, maturity, q);
            });
        }

        [ExcelFunction(Description = "Compute vanilla option greek within Bachelier model", 
                       Category = "PragmaticQuant_VanillaOption")]
        public static object BachelierGreek(double fwd, double strike, double maturity, double vol, string request)
        {
            return FunctionRunnerUtils.Run("BachelierGreek", () =>
            {
                double gamma, theta, vega, vanna, vomma;
                Bachelier.Greeks(fwd, strike, maturity, vol,
                    out gamma, out theta, out vega, out vanna, out vomma);
                switch (request.Trim().ToLower())
                {
                    case "gamma":
                        return gamma;
                    case "theta":
                        return theta;
                    case "vega":
                        return vega;
                    case "vanna":
                        return vanna;
                    case "vomma":
                        return vomma;
                    default:
                        throw new Exception(string.Format("Unknow greek : {0}", request));
                }
            });
        }

        [ExcelFunction(Description = "Compute vanilla option price within Black-Scholes model", 
                       Category = "PragmaticQuant_VanillaOption")]
        public static object BlackOption(double forward, double strike, double maturity, double vol, string optionType)
        {
            return FunctionRunnerUtils.Run("BlackOption", () =>
            {
                double q;
                switch (optionType.Trim().ToLower())
                {
                    case "call":
                        q = 1.0;
                        break;
                    case "put":
                        q = -1.0;
                        break;
                    default:
                        throw new Exception(string.Format("Unknow option type : {0}", optionType));
                }

                return BlackScholesOption.Price(forward, strike, vol, maturity, q);
            });
        }

        [ExcelFunction(Description = "Compute vanilla option implied vol within Black-Scholes model", 
                       Category = "PragmaticQuant_VanillaOption")]
        public static object BlackImpliedVol(double forward, double strike, double maturity, double price, string optionType)
        {
            return FunctionRunnerUtils.Run("BlackImpliedVol", () =>
            {
                double q;
                switch (optionType.Trim().ToLower())
                {
                    case "call":
                        q = 1.0;
                        break;
                    case "put":
                        q = -1.0;
                        break;
                    default:
                        throw new Exception(string.Format("Unknow option type : {0}", optionType));
                }
                return BlackScholesOption.ImpliedVol(price, forward, strike, maturity, q);
            });
        }

        [ExcelFunction(Description = "Compute vanilla option greek within Black-Scholes model", 
                       Category = "PragmaticQuant_VanillaOption")]
        public static object BlackGreek(double fwd, double strike, double maturity, double vol, string request)
        {
            return FunctionRunnerUtils.Run("BlackGreek", () =>
            {
                double gamma, theta, vega, vanna, vomma;
                BlackScholesOption.Greeks(fwd, strike, maturity, vol,
                    out gamma, out theta, out vega, out vanna, out vomma);
                switch (request.Trim().ToLower())
                {
                    case "gamma":
                        return gamma;
                    case "theta":
                        return theta;
                    case "vega":
                        return vega;
                    case "vanna":
                        return vanna;
                    case "vomma":
                        return vomma;
                    default:
                        throw new Exception(string.Format("Unknow greek : {0}", request));
                }
            });
        }
    }
}
