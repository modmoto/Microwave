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
            Id = Guid.NewGuid();
        }

        // Todo find a way to fill them from json parsing
        public Guid Id { get; set; }
        public Guid EntityId { get; set; }
    }
}