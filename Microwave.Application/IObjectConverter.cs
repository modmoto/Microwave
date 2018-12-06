namespace Microwave.Application.Ports
{
    public interface IObjectConverter
    {
        string Serialize<T>(T objectToSerialize);
        T Deserialize<T>(string payLoad);
    }
}