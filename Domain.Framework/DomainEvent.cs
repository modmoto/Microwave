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

        public Guid Id { get; private set;  }
        [ActualPropertyName(nameof(Entity.Id))]
        public Guid EntityId { get; private set;  }
    }
}