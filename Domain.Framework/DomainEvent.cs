using System;

namespace Domain.Framework
{
    public abstract class DomainEvent
    {
        protected DomainEvent(Guid entityId)
        {
            EntityId = entityId;
            Id = Guid.NewGuid();
        }

        public Guid Id { get; }
        public Guid EntityId { get; set; }
    }
}