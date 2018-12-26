using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.Domain;
using Microwave.Queries;
using Microwave.TestHostRead.Querries;

namespace Microwave.TestHostRead.Controllers
{
    [Route("api/teams")]
    public class TeamQuerryController : Controller
    {
        private readonly IReadModelRepository _queryRepository;

        public TeamQuerryController(IReadModelRepository queryRepository)
        {
            _queryRepository = queryRepository;
        }

        [HttpGet("{teamId}")]
        public async Task<ActionResult> GetTeam(Guid teamId)
        {
            var teamQuerry = await _queryRepository.Load<TeamReadModel>(GuidIdentity.Create(teamId));
            return Ok(teamQuerry.Value);
        }

        [HttpGet("Count")]
        public async Task<ActionResult> GetCounter()
        {
            var teamQuerry = await _queryRepository.Load<CounterQuery>();
            return Ok(teamQuerry.Value);
        }
    }
}