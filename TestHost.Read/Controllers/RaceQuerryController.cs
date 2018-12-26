using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.Domain;
using Microwave.Queries;
using TestHost.Read.Querries;

namespace TestHost.Read.Controllers
{
    [Route("api/races")]
    public class RaceQuerryController : Controller
    {
        private readonly IReadModelRepository _queryRepository;

        public RaceQuerryController(IReadModelRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        [HttpGet("{raceId}")]
        public async Task<ActionResult> GetTeam(string raceId)
        {
            var teamQuerry = await _queryRepository.Load<RaceReadModel>(StringIdentity.Create(raceId));
            return Ok(teamQuerry.Value);
        }
    }
}