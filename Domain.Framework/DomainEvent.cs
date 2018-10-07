using System;

namespace Domain.Framework
{
    public abstract class DomainEvent
    {
        private DomainEvent()
        {
        }

        protected DomainEvent(Guid entityId)
        {
            EntityId = entityId;
            DomainEventId = Guid.NewGuid();
        }

        [ActualPropertyName(nameof(Entity.Id))]
        public Guid EntityId { get; }
        public Guid DomainEventId { get; }
    }
}