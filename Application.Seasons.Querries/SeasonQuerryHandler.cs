using System;
using System.Collections.Generic;
using Application.Framework;
using Domain.Seasons.Events;

namespace Application.Seasons.Querries
{
    public class SeasonQuerryHandler : QuerryHandler<AllSeasonsQuery>
    {
        public SeasonQuerryHandler(IObjectPersister<AllSeasonsQuery> persister) : base(persister)
        {
        }

        public IEnumerable<SeasonDto> GetAllSeasons()
        {
            return AllSeasons;
        }

        public IEnumerable<SeasonDto> AllSeasons { get; }
    }

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
    }
}