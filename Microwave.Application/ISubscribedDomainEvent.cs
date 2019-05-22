using Microwave.Domain;

namespace Microwave.Application
{
    public interface ISubscribedDomainEvent
    {
        Identity EntityId { get; }
    }
}