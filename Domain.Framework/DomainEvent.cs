using System;

namespace Domain.Framework
{
    public abstract class DomainEvent
    {
        protected DomainEvent(Guid entityId)
        {
            EntityId = entityId;
            DomainEventId = Guid.NewGuid();
        }

        [ActualPropertyName(nameof(Entity.Id))]
        public Guid EntityId { get; set; }
        public Guid DomainEventId { get; set; }
    }
}