namespace pragmatic_quant_com.Publishers
{
    public interface IPublisher<in T>
    {
        object[,] Publish(T obj);
    }
}