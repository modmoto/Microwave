using Domain.Framework;
using EventStore.ClientAPI;

namespace Application.Framework
{
    public interface IDomainEventConverter
    {
        string Serialize(DomainEvent eve);
        DomainEvent Deserialize(ResolvedEvent eve);
    }
}