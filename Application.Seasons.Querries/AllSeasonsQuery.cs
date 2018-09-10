using System.Collections.Generic;
using Application.Framework;
using Domain.Seasons.Events;

namespace Application.Seasons.Querries
{
    public class AllSeasonsQuery : Query
    {
        public IList<SeasonDto> Seasons { get; } = new List<SeasonDto>();

        public void Apply(SeasonCreatedEvent createdEvent)
        {
            var season = new SeasonDto();
            season.Apply(createdEvent);
            Seasons.Add(season);
        }

        public void Apply(SeasonNameChangedEvent nameChangedEvent)
        {
            foreach (var season in Seasons)
            {
                if (season.Id == nameChangedEvent.EntityId) season.Apply(nameChangedEvent);
            }
        }

    }
}