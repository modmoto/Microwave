using System.Collections.Generic;

namespace Microwave.EventStores
{
    public interface IApply
    {
        void Apply(IEnumerable<IDomainEvent> domainEvents);
    }

    public interface IApply<in T> where T : IDomainEvent
    {
        void Apply(T domainEvent);
    }
}