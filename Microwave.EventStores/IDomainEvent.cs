using Microwave.Domain;

namespace Microwave.EventStores
{
    public interface IDomainEvent
    {
        Identity EntityId { get; }
    }
}