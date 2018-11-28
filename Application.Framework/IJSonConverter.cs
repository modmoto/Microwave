namespace Microwave.Application
{
    public interface IJSonConverter<T>
    {
        string Serialize(T eve);
        T Deserialize(string payLoad);
    }
}