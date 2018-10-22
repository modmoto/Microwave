using System.Collections.Generic;
using Domain.Framework;

namespace Application.Framework
{
    public interface IDomainEventConverter
    {
        string Serialize<T>(T eve) where T : DomainEvent;
        T Deserialize<T>(string payLoad);
        IEnumerable<T> DeserializeList<T>(string payLoad);
    }
}