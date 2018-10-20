using EventStore.ClientAPI;

namespace Application.Framework
{
    public interface IJSonConverter<T>
    {
        string Serialize(T eve);
        T Deserialize(ResolvedEvent domainEvent);
        T Deserialize(string payLoad);
    }
}