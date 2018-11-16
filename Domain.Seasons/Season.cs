using System;
using Domain.Framework;
using Domain.Seasons.Events;

namespace Domain.Seasons
{
    public class Season
    {
        public Guid Id { get; set; }
        public string SeasonName { get; private set; }

        public static DomainResult Create(string seasonName)
        {
            var seasonCreatedEvent = new SeasonCreatedEvent(Guid.NewGuid(), seasonName);
            return DomainResult.Ok(seasonCreatedEvent);
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
            if (commandName.Contains("Sex")) return DomainResult.Error(new []{ "Name can not contain Sex" });
            var seasonNameChangedEvent = new SeasonNameChangedEvent(Id, commandName);
            Apply(seasonNameChangedEvent);
            return DomainResult.Ok(seasonNameChangedEvent);
        }
    }
}