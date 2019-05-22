using Microwave.Domain;

namespace Microwave.Queries
{
    public interface ISubscribedDomainEvent
    {
        Identity EntityId { get; }
    }
}