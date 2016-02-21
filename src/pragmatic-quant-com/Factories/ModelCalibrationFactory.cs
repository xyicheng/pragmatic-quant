using System;
using pragmatic_quant_model.Basic.Dates;
using pragmatic_quant_model.Basic.Structure;
using pragmatic_quant_model.Model;
using pragmatic_quant_model.Model.Equity.Bergomi;
using pragmatic_quant_model.Model.Equity.BlackScholes;
using pragmatic_quant_model.Model.Equity.LocalVolatility;

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
                case "bergomi2f":
                    return Bergomi2FModelFactoryFromBag.Instance.Build(bag);
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
            
            var calibDatas = bag.ProcessTimeMatrixDatas("CalibDate");

            if (calibDatas.HasCol("Sigma"))
            {
                var sigmaValues = calibDatas.GetColFromLabel("Sigma");
                var sigma = new MapRawDatas<DateOrDuration, double>(calibDatas.RowLabels, sigmaValues);
                var bsDescription = new BlackScholesModelDescription(assetName, sigma, withDivs);
                return new ExplicitCalibration<BlackScholesModelDescription>(bsDescription);
            }
            
            var calibStrikes = calibDatas.GetColFromLabel("Strike");
            return new BlackScholesModelCalibDesc(assetName, withDivs, calibDatas.RowLabels, calibStrikes);
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

    public class Bergomi2FModelFactoryFromBag :
        Singleton<Bergomi2FModelFactoryFromBag>, IFactoryFromBag<ICalibrationDescription>
    {
        public ICalibrationDescription Build(object[,] bag)
        {
            string assetName = bag.ProcessScalarString("Asset");
            bool withDivs = !bag.Has("WithDivs") || bag.ProcessScalarBoolean("WithDivs");

            var calibDatas = bag.ProcessTimeMatrixDatas("Date");
            var sigmaValues = calibDatas.GetColFromLabel("Sigma");
            var sigma = new MapRawDatas<DateOrDuration, double>(calibDatas.RowLabels, sigmaValues);
            
            double k1 = bag.ProcessScalarDouble("K1");
            double k2 = bag.ProcessScalarDouble("K2");
            double theta = bag.ProcessScalarDouble("Theta");
            double nu = bag.ProcessScalarDouble("Nu");
            double rhoXY = bag.ProcessScalarDouble("RhoXY");
            double rhoSX = bag.ProcessScalarDouble("RhoSX");
            double rhoSY = bag.ProcessScalarDouble("RhoSY");

            Bergomi2FModelDescription b2FDescription = new Bergomi2FModelDescription(assetName, withDivs, sigma,
                k1, k2, theta, nu, rhoXY, rhoSX, rhoSY);

            return new ExplicitCalibration<Bergomi2FModelDescription>(b2FDescription);
        }
    }


}