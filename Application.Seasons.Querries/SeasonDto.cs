using System;
using Domain.Seasons.Events;

namespace Application.Seasons.Querries
{
    public class SeasonDto
    {
        public string Name { get; set; }

        public DateTimeOffset StartDate { get; set; }

        public DateTimeOffset EndDate { get; set; }

        public Guid Id { get; set; }


        public void Apply(SeasonCreatedEvent domaintEvent)
        {
            Id = domaintEvent.EntityId;
            Name = domaintEvent.InitialName;
        }

        public void Apply(SeasonDateChangedEvent domainEvent)
        {
            StartDate = domainEvent.StartDate;
            EndDate = domainEvent.EndDate;
        }

        public void Apply(SeasonNameChangedEvent nameChangedEvent)
        {
            Name = nameChangedEvent.Name;
        }
    }
}