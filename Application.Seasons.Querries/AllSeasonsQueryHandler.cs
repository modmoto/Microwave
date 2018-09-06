using System;
using System.Linq;
using Application.Framework;
using Domain.Seasons.Events;

namespace Application.Seasons.Querries
{
    public class AllSeasonsQueryHandler : QueryHandler<AllSeasonsQuery>
    {
        public AllSeasonsQuery GetAllSeasons()
        {
            return QueryObject;
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