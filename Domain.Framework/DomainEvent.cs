using System;

namespace Domain.Framework
{
    public abstract class DomainEvent
    {
        public DomainEvent(Guid entityId)
        {
            EntityId = entityId;
            DomainEventId = Guid.NewGuid();
        }

        public Guid EntityId { get; }
        public Guid DomainEventId { get; set; }
        public long Version { get; set; }
        public long Created { get; set; }
        public string Type => GetType().Name;
    }
}