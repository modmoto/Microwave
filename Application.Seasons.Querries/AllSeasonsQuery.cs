using System.Collections.Generic;
using Application.Framework;
using Domain.Framework;
using Domain.Seasons.Events;

namespace Application.Seasons.Querries
{
    public class AllSeasonsQuery : Querry
    {
        public IList<SeasonDto> Seasons { get; set; }

        public void Apply(SeasonCreatedEvent createdEvent)
        {
            var season = new SeasonDto();
            season.Apply(createdEvent);
            Seasons.Add(season);
        }

        public void Apply(SeasonDateChangedEvent createdEvent)
        {
            foreach (var season in Seasons)
            {
                if (season.Id == createdEvent.EntityId) season.Apply(createdEvent);
            }
        }
    }
}