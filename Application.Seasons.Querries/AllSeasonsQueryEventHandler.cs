using System;
using System.Collections.Generic;
using System.Linq;
using Application.Framework;

namespace Application.Seasons.Querries
{
    public class AllSeasonsQueryEventHandler : QueryEventHandler<AllSeasonsQuery>
    {
        public AllSeasonsQueryEventHandler(AllSeasonsQuery queryObject,
            SubscribedEventTypes<AllSeasonsQuery> eventTypes) : base(queryObject, eventTypes)
        {
        }

        public IEnumerable<SeasonDto> GetAllSeasons()
        {
            return QueryObject.Seasons;
        }

        public SeasonDto GetSeason(Guid id)
        {
            return QueryObject.Seasons.SingleOrDefault(season => season.Id == id);
        }
    }

    public class AllSeasonsCounterQueryHandler : QueryEventHandler<AllSeasonsCounterQuery>
    {
        public AllSeasonsCounterQueryHandler(AllSeasonsCounterQuery queryObject,
            SubscribedEventTypes<AllSeasonsCounterQuery> eventTypes) : base(queryObject, eventTypes)
        {
        }

        public long GetSeasonCount()
        {
            return QueryObject.Counter;
        }
    }
}