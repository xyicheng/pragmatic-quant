using System;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.Maths;
using pragmatic_quant_model.Maths.Sobol;
using pragmatic_quant_model.Model;

namespace pragmatic_quant_com.Factories
{
    public class AlgorithmFactory
        : Singleton<AlgorithmFactory>, IFactoryFromBag<INumericalMethodConfig>
    {
        #region private fields
        private static MonteCarloConfig BuildMonteCarloConfig(object[,] bag)
        {
            int nbPaths = bag.ProcessScalarInteger("NbPaths");
            Duration mcStep = bag.Has("McStep") ? bag.ProcessScalarDateOrDuration("McStep").Duration : null;
            
            //TODO investigate which generator is the best
            var randomGenerator = RandomGenerators.GaussianSobol(SobolDirection.Kuo3);

            return new MonteCarloConfig(nbPaths, randomGenerator, mcStep);
        }
        #endregion
        public INumericalMethodConfig Build(object[,] bag)
        {
            var algorithm = bag.ProcessScalarString("Algorithm");
            switch (algorithm.ToLowerInvariant())
            {
                case "montecarlo":
                    return BuildMonteCarloConfig(bag);

                case "ls":
                    throw new NotImplementedException("Longstaff Schwartz");

                case "pde":
                    throw new NotImplementedException("Pde");
            }

            throw new ArgumentException(string.Format("Unknown algorithm : {0} ", algorithm));
        }
    }
}