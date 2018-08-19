using System.Collections.Generic;
using Domain.Framework;
using Domain.Seasons;

namespace Application.Seasons
{
    public class AllSeasonsQuery : Entity
    {
        public IList<Season> Seasons { get; set; }

        public void Apply(SeasonCreatedEvent createdEvent)
        {
            var season = new Season();
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