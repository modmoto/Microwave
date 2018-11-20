namespace Application.Framework
{
    public interface IObjectConverter
    {
        string Serialize<T>(T objectToSerialize);
        T Deserialize<T>(string payLoad);
    }
}