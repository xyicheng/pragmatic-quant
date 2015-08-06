using pragmatic_quant_model.MarketDatas;

namespace pragmatic_quant_model.Model
{
    public class ExplicitCalibration<T> : ICalibrationDescription
        where T : IModelDescription
    {
        public ExplicitCalibration(T model)
        {
            Model = model;
        }
        public T Model { get; private set; }
    }

    public class ExplicitModelCalibration<T> : ModelCalibration<ExplicitCalibration<T>>
        where T : IModelDescription
    {
        public override IModelDescription Calibrate(ExplicitCalibration<T> calibDesc, Market market)
        {
            return calibDesc.Model;
        }
    }

}