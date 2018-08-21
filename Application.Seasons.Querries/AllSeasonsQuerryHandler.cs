using System;
using System.Linq;
using Application.Framework;

namespace Application.Seasons.Querries
{
    public class AllSeasonsQuerryHandler : QuerryHandler<AllSeasonsQuery>
    {
        public AllSeasonsQuery GetAllSeasons()
        {
            return QuerryObject;
        }

        public SeasonDto GetSeason(Guid id)
        {
            return QuerryObject.Seasons.SingleOrDefault(season => season.Id == id);
        }

        public AllSeasonsQuerryHandler(AllSeasonsQuery querryObject) : base(querryObject)
        {
        }
    }
}