namespace Microwave.Application
{
    public interface IObjectConverter
    {
        string Serialize<T>(T objectToSerialize);
        T Deserialize<T>(string payLoad);
    }
}