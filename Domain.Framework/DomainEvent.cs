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
        // TODO as wrapper
        public long Version { get; private set; }
        public long Created { get; private set; }

        public void MarkNow(int version)
        {
            Version = version;
            Created = DateTime.UtcNow.Ticks;
        }
    }
}