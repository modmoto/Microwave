using System;
using System.Threading.Tasks;
using Application.Seasons;
using Application.Seasons.Commands;
using Application.Seasons.Querries;
using Microsoft.AspNetCore.Mvc;

namespace Adapters.WebApi.Seasons
{
    [Route("Api/Seasons")]
    public class SeasonController : Controller
    {
        private readonly SeasonCommandHandler _commandHandler;
        private readonly AllSeasonsQueryQuerryEventHandler _querryEventHandler;

        public SeasonController(SeasonCommandHandler commandHandler, AllSeasonsQueryQuerryEventHandler querryEventHandler)
        {
            _commandHandler = commandHandler;
            _querryEventHandler = querryEventHandler;
        }

        [HttpGet]
        public ActionResult GetSeasons()
        {
            var seasons = _querryEventHandler.GetAllSeasons();
            return Ok(seasons);
        }

        [HttpGet("{entityId}")]
        public ActionResult GetSeason(Guid entityId)
        {
            var seasons = _querryEventHandler.GetSeason(entityId);
            return Ok(seasons);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateSeason([FromBody] CreateSesonCommand command)
        {
            await _commandHandler.CreateSeason(command);
            return Ok();
        }

        [HttpPut("{entityId}/ChangeName")]
        public async Task<IActionResult> ChangeName(Guid entityId, [FromBody] ChangeNameApiCommand command)
        {
            var changeDateCommand = new ChangeNameCommand
            {
                EntityId = entityId,
                Name = command.Name
            };

            await _commandHandler.ChangeName(changeDateCommand);
            return Ok();
        }
    }

    public class ChangeNameApiCommand
    {
        public string Name { get; set; }
    }
}