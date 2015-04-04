namespace pragmatic_quant_model.Factories
{
    interface IFactoryFromBag<out T>
    {
        T Build(object[,] bag);
    }
}