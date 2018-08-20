using System;
using Domain.Framework;

namespace Domain.Seasons.Events
{
    public class SeasonCreatedEvent : DomainEvent
    {
        public string InitialName { get; }
        public int MaxDaysBetweenGames { get; }

        public SeasonCreatedEvent(Guid entityId, string initialName, int daysBetweenGames) : base(entityId)
        {
            InitialName = initialName;
            MaxDaysBetweenGames = daysBetweenGames;
        }
    }
}