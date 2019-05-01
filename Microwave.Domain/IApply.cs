using System.Collections.Generic;

namespace Microwave.Domain
{
    public interface IApply
    {
        void Apply(IEnumerable<IDomainEvent> domainEvents);
    }

    public interface IApply<in T>
    {
        void Apply(T domainEvent);
    }
}