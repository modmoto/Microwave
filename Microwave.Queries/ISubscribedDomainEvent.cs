using Microwave.Domain.Identities;

namespace Microwave.Queries
{
    public interface ISubscribedDomainEvent
    {
        Identity EntityId { get; }
    }
}