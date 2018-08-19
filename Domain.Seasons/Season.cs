using System;
using Domain.Framework;

namespace Domain.Seasons
{
    public class Season : Entity
    {
        public void Apply(SeasonCreatedEvent domainEvent)
        {
            Id = domainEvent.EntityId;
        }
    }

    public class SeasonCreatedEvent : DomainEvent
    {
        public SeasonCreatedEvent(Guid entityId) : base(entityId)
        {
        }
    }
}