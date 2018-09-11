using EventStore.ClientAPI;

namespace Application.Framework
{
    public interface IJSonConverter<T>
    {
        string Serialize(T eve);
        T Deserialize(ResolvedEvent domainEvent);
    }
}