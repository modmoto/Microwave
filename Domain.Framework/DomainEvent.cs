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
        public long Version { get; private set; }
        public long Created { get; private set; }
        public string Type => GetType().Name;

        public void MarkNow(int version)
        {
            Version = version;
            Created = DateTime.UtcNow.Ticks;
        }
    }
}