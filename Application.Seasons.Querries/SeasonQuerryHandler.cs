using System.Threading.Tasks;
using Application.Framework;

namespace Application.Seasons.Querries
{
    public class SeasonQuerryHandler : QuerryHandler<AllSeasonsQuery>
    {
        public SeasonQuerryHandler(IObjectPersister<AllSeasonsQuery> persister) : base(persister)
        {
        }

        public async Task<AllSeasonsQuery> GetAllSeasons()
        {
            return await _objectPersister.GetAsync();
        }
    }
}