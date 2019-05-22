using Microwave.Domain.Identities;

namespace Microwave.Domain.EventSourcing
{
    public interface IDomainEvent
    {
        Identity EntityId { get; }
    }
}