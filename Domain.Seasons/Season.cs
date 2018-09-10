using System;
using Domain.Framework;
using Domain.Seasons.Events;

namespace Domain.Seasons
{
    public class Season : Entity
    {
        public string SeasonName { get; private set; }

        public static DomainResult Create(string seasonName)
        {
            var seasonCreatedEvent = new SeasonCreatedEvent(Guid.NewGuid(), seasonName);
            return DomainResult.OkResult(seasonCreatedEvent);
        }

        public void Apply(SeasonCreatedEvent domainEvent)
        {
            SeasonName = domainEvent.InitialName;
            Id = domainEvent.EntityId;
        }

        public void Apply(SeasonNameChangedEvent domainEvent)
        {
            SeasonName = domainEvent.Name;
        }

        public DomainResult ChangeName(string commandName)
        {
            return DomainResult.OkResult(new SeasonNameChangedEvent(Id, commandName));
        }
    }
}