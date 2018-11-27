using System;
using Domain.Framework;

namespace Domain.Seasons.DomainEvents
{
    public class SeasonNameChangedEvent : IDomainEvent
    {
        public SeasonNameChangedEvent(Guid entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }

        public string Name { get; }
        public Guid EntityId { get; }
    }
}