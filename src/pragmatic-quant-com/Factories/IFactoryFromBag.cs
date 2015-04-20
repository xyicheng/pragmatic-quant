namespace pragmatic_quant_com.Factories
{
    interface IFactoryFromBag<out T>
    {
        T Build(object[,] bag);
    }
}