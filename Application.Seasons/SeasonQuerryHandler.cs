using System.Collections.Generic;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using Domain.Seasons;

namespace Application.Seasons
{
    public class SeasonQuerryHandler : QuerryHandler<AllSeasonsQuery>
    {
        public SeasonQuerryHandler(IObjectPersister<AllSeasonsQuery> persister) : base(persister)
        {
        }

        public IEnumerable<Season> GetAllSeasons()
        {
            return AllSeasons;
        }

        public IEnumerable<Season> AllSeasons { get; }
    }
}