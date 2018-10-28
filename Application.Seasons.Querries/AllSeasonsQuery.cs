using System.Collections.Generic;
using Application.Framework;
using Domain.Framework;
using Domain.Seasons.Events;

namespace Application.Seasons.Querries
{
    public class AllSeasonsQuery : Query, IHandle<SeasonCreatedEvent>, IHandle<SeasonNameChangedEvent>
    {
        public IList<SeasonDto> Seasons { get; } = new List<SeasonDto>();

        public void Handle(SeasonCreatedEvent createdEvent)
        {
            var season = new SeasonDto();
            season.Apply(createdEvent);
            Seasons.Add(season);
        }

        public void Handle(SeasonNameChangedEvent nameChangedEvent)
        {
            foreach (var season in Seasons)
            {
                if (season.Id == nameChangedEvent.EntityId) season.Apply(nameChangedEvent);
            }
        }
    }

    public interface IHandle<T> where T : DomainEvent
    {
        void Handle(T domainEvent);
    }
}