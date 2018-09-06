using System;
using System.Collections.Generic;
using System.Linq;
using Application.Framework;

namespace Application.Seasons.Querries
{
    public class AllSeasonsQueryHandler : QueryHandler<AllSeasonsQuery>
    {
        public IEnumerable<SeasonDto> GetAllSeasons()
        {
            return QueryObject.Seasons;
        }

        public SeasonDto GetSeason(Guid id)
        {
            return QueryObject.Seasons.SingleOrDefault(season => season.Id == id);
        }

        public AllSeasonsQueryHandler(AllSeasonsQuery queryObject, SubscribedEventTypes<AllSeasonsQuery> eventTypes) : base(queryObject, eventTypes)
        {
        }
    }
}