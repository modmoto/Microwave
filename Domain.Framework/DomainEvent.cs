using System;

namespace Domain.Framework
{
    public abstract class DomainEvent
    {
        protected DomainEvent(Guid entityId)
        {
            EntityId = entityId;
        }

        [ActualPropertyName(nameof(Entity.Id))]
        public Guid EntityId { get; private set;  }
    }
}