using System;

namespace Microwave.Domain
{
    public interface IDomainEvent
    {
        Guid EntityId { get; }
    }

    public interface IApply<in T> where T : IDomainEvent
    {
        void Apply(T domainEvent);
    }
}