using System;

namespace Domain.Framework
{
    public abstract class DomainEvent
    {
        public DomainEvent(Guid entityId)
        {
            EntityId = entityId;
        }

        public Guid EntityId { get; }
        public long Version { get; set; }
        public long Created { get; set; }
        public string DomainEventType => GetType().Name;
    }
}