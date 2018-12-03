using System.Collections.Generic;

namespace Microwave.Domain
{
    public interface IApply
    {
        void Apply(IEnumerable<IDomainEvent> domainEvents);
    }
}