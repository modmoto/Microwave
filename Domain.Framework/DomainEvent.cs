using System;

namespace Domain.Framework
{
    //TODO as interface
    public abstract class DomainEvent
    {
        public DomainEvent(Guid entityId)
        {
            EntityId = entityId;
        }

        public Guid EntityId { get; }
    }
}