using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.Domain;
using TestHost.Write.Handler;

namespace TestHost.Write.Controller
{
    [Route("api/teams")]
    public class TeamController : Microsoft.AspNetCore.Mvc.Controller
    {
        private readonly TeamCommandHandler _commandHandler;

        public TeamController(TeamCommandHandler commandHandler)
        {
            _commandHandler = commandHandler;
        }

        [HttpPost("create")]
        public async Task<ActionResult> CreateTeam([FromBody] CreateTeamCommand createTeamCommand)
        {
            var teamGuid = await _commandHandler.CreateTeam(createTeamCommand);
            return Created($"Api/Teams/{teamGuid}", null);
        }

        [HttpPost("{teamId}/buyPlayer")]
        public async Task<ActionResult> BuyPlayer(Guid teamId, [FromBody] BuyPlayerCommand createTeamCommand)
        {
            createTeamCommand.TeamId = GuidIdentity.Create(teamId);
            await _commandHandler.BuyPlayer(createTeamCommand);
            return Ok();
        }
    }
}