using System.Collections.Generic;

namespace Microwave.Domain
{
    public interface IApply
    {
        void Apply(IEnumerable<IDomainEvent> domainEvents);
    }

    public interface IApply<T> where T : IDomainEvent
    {
        void Apply(T domainEvent);
    }
}