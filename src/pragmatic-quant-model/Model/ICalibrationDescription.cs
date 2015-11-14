using System;
using System.Collections.Generic;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.MarketDatas;
using pragmatic_quant_model.Model.Equity.BlackScholes;
using pragmatic_quant_model.Model.Equity.LocalVolatility;
using pragmatic_quant_model.Model.HullWhite;

namespace pragmatic_quant_model.Model
{
    public interface ICalibrationDescription
    {}

    public interface IModelCalibration
    {
        IModelDescription Calibrate(ICalibrationDescription calibDescription, Market market);
    }

    public class ModelCalibration : Singleton<ModelCalibration>, IModelCalibration
    {
        #region private fields
        private static readonly IDictionary<Type, IModelCalibration> factories = GetFactories();
        #endregion
        #region private methods
        private static void AddExplicit<T>(IDictionary<Type, IModelCalibration> dico)
            where T : IModelDescription 
        {
            dico.Add(typeof(ExplicitCalibration<T>), new ExplicitModelCalibration<T>());
        }
        private static IDictionary<Type, IModelCalibration> GetFactories()
        {
            var result = new Dictionary<Type, IModelCalibration>();
            AddExplicit<Hw1ModelDescription>(result);
            AddExplicit<BlackScholesModelDescription>(result);
            result.Add(typeof (LocalVolModelCalibDesc), LocalVolModelCalibration.Instance);
            result.Add(typeof(BlackScholesModelCalibDesc), BlackScholesModelCalibration.Instance);
            return result;
        }
        #endregion

        public IModelDescription Calibrate(ICalibrationDescription calibDescription, Market market)
        {
            IModelCalibration modelfactory;
            if (factories.TryGetValue(calibDescription.GetType(), out modelfactory))
                return modelfactory.Calibrate(calibDescription, market);
            throw new ArgumentException(string.Format("Missing Model Calibration for {0}", calibDescription));
        }
    }
    
    public abstract class ModelCalibration<TCalib> : IModelCalibration
        where TCalib : class, ICalibrationDescription
    {
        public abstract IModelDescription Calibrate(TCalib calibDesc, Market market);
        public IModelDescription Calibrate(ICalibrationDescription calibDescription, Market market)
        {
            var calibDescImplem = calibDescription as TCalib;
            if (calibDescImplem == null)
                throw new Exception(string.Format("ModelCalibration : {0} expected but was {1}",
                                                  typeof (TCalib), calibDescription.GetType()));
            return Calibrate(calibDescImplem, market);
        }
    }

}