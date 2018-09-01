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

        // Todo find a way to fill them from json parsing
        public Guid Id { get; set; }
        [ActualPropertyName(nameof(Entity.Id))]
        public Guid EntityId { get; set; }
    }
}