using System;

namespace Domain.Framework
{
    public interface IDomainEvent
    {
        Guid EntityId { get; }
    }
}