using System.Collections.Generic;
using Domain.Framework;

namespace Application.Framework
{
    public interface IObjectConverter
    {
        string Serialize<T>(T eve);
        T Deserialize<T>(string payLoad);
        IEnumerable<T> DeserializeList<T>(string payLoad);
    }
}