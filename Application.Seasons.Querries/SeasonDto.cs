using System;
using Domain.Seasons.DomainEvents;

namespace Application.Seasons.Querries
{
    public class SeasonDto
    {
        public string Name { get; set; }

        public Guid Id { get; set; }


        public void Apply(SeasonCreatedEvent domaintEvent)
        {
            Id = domaintEvent.EntityId;
            Name = domaintEvent.InitialName;
        }

        public void Apply(SeasonNameChangedEvent nameChangedEvent)
        {
            Name = nameChangedEvent.Name;
        }
    }
}