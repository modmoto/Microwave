using System;
using Domain.Framework;

namespace Domain.Seasons.Events
{
    public class SeasonNameChangedEvent : DomainEvent
    {
        public SeasonNameChangedEvent(Guid entityId, string name) : base(entityId)
        {
            Name = name;
        }

        public string Name { get; }
    }
}