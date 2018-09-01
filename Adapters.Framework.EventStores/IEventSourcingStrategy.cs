using Domain.Framework;

namespace Adapters.Framework.EventStores
{
    public interface IEventSourcingStrategy
    {
        T Apply<T>(T entity, DomainEvent domainEvent);
    }
}