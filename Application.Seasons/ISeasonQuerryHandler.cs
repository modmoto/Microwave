using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Seasons;

namespace Application.Seasons
{
    public class SeasonQuerryHandler : IQuerryHandler<SeasonCreatedEvent>, IQuerryHandler<SeasonDateChangedEvent>
    {
        public SeasonQuerryHandler(Io)
        {

        }

        public Task Handle(SeasonCreatedEvent createdEvent)
        {

        }

        public IEnumerable<Season> GetAllSeasons()
        {
            return AllSeasons;
        }

        public IEnumerable<Season> AllSeasons { get; }

        public Task Handle(SeasonDateChangedEvent domainEvent)
        {

        }
    }
}