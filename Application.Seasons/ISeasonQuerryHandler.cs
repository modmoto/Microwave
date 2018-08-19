using System.Collections.Generic;
using System.Threading.Tasks;
using Domain.Seasons;

namespace Application.Seasons
{
    public interface ISeasonQuerryHandler
    {
        Task<IEnumerable<Season>> GetAllSeasons();
    }
}