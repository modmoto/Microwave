using System;
using Domain.Framework;

namespace Domain.Seasons.Events
{
    public class SeasonCreatedEvent : IDomainEvent
    {
        public string InitialName { get; }

        public SeasonCreatedEvent(Guid entityId, string initialName)
        {
            EntityId = entityId;
            InitialName = initialName;
        }

        public Guid EntityId { get; }
    }
}