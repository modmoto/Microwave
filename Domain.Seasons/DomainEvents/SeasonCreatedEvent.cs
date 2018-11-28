using System;
using Microwave.Domain;

namespace Domain.Seasons.DomainEvents
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