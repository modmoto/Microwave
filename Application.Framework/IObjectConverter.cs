using System.Collections.Generic;

namespace Application.Framework
{
    public interface IObjectConverter
    {
        string Serialize<T>(T objectToSerialize);
        T Deserialize<T>(string payLoad);
        IEnumerable<T> DeserializeList<T>(string payLoad);
    }
}