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
        private readonly SeasonQuerryHandler _seasonQuerryHandler;

        public SeasonController(SeasonCommandHandler commandHandler, SeasonQuerryHandler seasonQuerryHandler)
        {
            _commandHandler = commandHandler;
            _seasonQuerryHandler = seasonQuerryHandler;
        }

        [HttpGet]
        public async Task<IActionResult> GetSeasons()
        {
            var seasons = await _seasonQuerryHandler.GetAllSeasons();
            return Ok(seasons);
        }

        [HttpGet("{entityId}")]
        public async Task<IActionResult> GetSeason(Guid id)
        {
            var seasons = await _seasonQuerryHandler.GetSeason(id);
            return Ok(seasons);
        }

        [HttpPost("Create")]
        public async Task<IActionResult> CreateSeason([FromBody] CreateSesonCommand command)
        {
            await _commandHandler.CreateSeason(command);
            return Ok();
        }

        [HttpPut("{entityId}/ChangeDate")]
        public async Task<IActionResult> CreateSeason(Guid entityId,[FromBody] ChangeDateApiCommand command)
        {
            var changeDateCommand = new ChangeDateCommand
            {
                EntityId = entityId,
                StartDate = command.StartDate,
                EndDate = command.EndDate
            };

            await _commandHandler.SetStartAndEndDate(changeDateCommand);
            return Ok();
        }
    }

    public class ChangeDateApiCommand
    {
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set; }
    }
}