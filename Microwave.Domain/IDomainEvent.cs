using System;

namespace Microwave.Domain
{
    public interface IDomainEvent
    {
        Guid EntityId { get; }
    }
}