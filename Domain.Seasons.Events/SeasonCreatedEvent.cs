using System;
using Domain.Framework;

namespace Domain.Seasons.Events
{
    public class SeasonCreatedEvent : DomainEvent
    {
        public string InitialName { get; }

        public SeasonCreatedEvent(Guid entityId, string initialName) : base(entityId)
        {
            InitialName = initialName;
        }
    }
}