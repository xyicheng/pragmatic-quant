using System;
using System.Linq;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.Model;
using pragmatic_quant_model.Model.BlackScholes;
using pragmatic_quant_model.Model.LocalVolatility;

namespace pragmatic_quant_com.Factories
{
    public class ModelCalibrationFactory
        : Singleton<ModelCalibrationFactory>, IFactoryFromBag<ICalibrationDescription>
    {
        public ICalibrationDescription Build(object[,] bag)
        {
            string modelName = bag.ProcessScalarString("ModelName");
            switch (modelName.ToLowerInvariant())
            {
                case "blackscholes" :
                    return BlackScholesModelFactory.Instance.Build(bag);
                case "localvol":
                    return LocalVolModelFactoryFromBag.Instance.Build(bag);
            }

            throw new NotImplementedException(modelName);
        }
    }

    public class BlackScholesModelFactory
        : Singleton<BlackScholesModelFactory>, IFactoryFromBag<ICalibrationDescription>
    {
        public ICalibrationDescription Build(object[,] bag)
        {
            var assetName = bag.ProcessScalarString("Asset");
            var withDivs = !bag.Has("WithDivs") || bag.ProcessScalarBoolean("WithDivs");
            
            var calibMatrix = bag.ProcessTimeMatrixDatas("CalibDate");

            if (calibMatrix.ColLabels.Count() != 1)
                throw new ArgumentException("Invalid BlackScholes calibration parameters");

            var calibLabel = calibMatrix.ColLabels.First();
            var calibDatas = calibMatrix.GetCol(calibLabel);

            switch (calibLabel.ToLowerInvariant().Trim())
            {
                case "sigma" :
                    var sigma = new MapRawDatas<DateOrDuration, double>(calibMatrix.RowLabels, calibDatas);
                    var bsDescription = new BlackScholesModelDescription(assetName, sigma, withDivs);
                    return new ExplicitCalibration<BlackScholesModelDescription>(bsDescription);
            }

            throw new ArgumentException(string.Format("Unknown BlackScholes parameter {0}", calibLabel));
        }
    }

    public class LocalVolModelFactoryFromBag
        : Singleton<LocalVolModelFactoryFromBag>, IFactoryFromBag<ICalibrationDescription>
    {
        public ICalibrationDescription Build(object[,] bag)
        {
            var assetName = bag.ProcessScalarString("Asset");
            var withDivs = !bag.Has("WithDivs") || bag.ProcessScalarBoolean("WithDivs");
            return new LocalVolModelCalibDesc(assetName, withDivs);
        }
    }


}