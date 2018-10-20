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
        private readonly AllSeasonsCounterQueryHandler _counterQueryHandler;
        private readonly AllSeasonsQueryEventHandler _eventHandler;

        public SeasonController(SeasonCommandHandler commandHandler, AllSeasonsCounterQueryHandler counterQueryHandler,
            AllSeasonsQueryEventHandler eventHandler)
        {
            _commandHandler = commandHandler;
            _eventHandler = eventHandler;
            _counterQueryHandler = counterQueryHandler;
        }

        [HttpGet]
        public ActionResult GetSeasons()
        {
            var seasons = _eventHandler.GetAllSeasons();
            return Ok(seasons);
        }

        [HttpGet("{entityId}")]
        public ActionResult GetSeason(Guid entityId)
        {
            var seasons = _eventHandler.GetSeason(entityId);
            return Ok(seasons);
        }

        [HttpGet("Counter")]
        public ActionResult GetSeasonsCount()
        {
            var counter = _counterQueryHandler.GetSeasonCount();
            return Ok(counter);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateSeason([FromBody] CreateSesonCommand command)
        {
            var guid = await _commandHandler.CreateSeason(command);
            return Ok(guid);
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