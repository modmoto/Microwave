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
        private readonly AllSeasonsQuerryHandler _querryHandler;

        public SeasonController(SeasonCommandHandler commandHandler, AllSeasonsQuerryHandler querryHandler)
        {
            _commandHandler = commandHandler;
            _querryHandler = querryHandler;
        }

        [HttpGet]
        public ActionResult GetSeasons()
        {
            var seasons = _querryHandler.GetAllSeasons();
            return Ok(seasons);
        }

        [HttpGet("{entityId}")]
        public ActionResult GetSeason(Guid id)
        {
            var seasons = _querryHandler.GetSeason(id);
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