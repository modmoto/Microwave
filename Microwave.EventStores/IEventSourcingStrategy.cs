using Microwave.Domain;

namespace Microwave.EventStores
{
    public interface IEventSourcingStrategy
    {
        T Apply<T>(T entity, IDomainEvent domainEvent);
    }
}