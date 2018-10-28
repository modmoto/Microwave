using System;
using System.Threading.Tasks;
using Application.Framework;
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
        private readonly IQeryRepository _qeryRepository;

        public SeasonController(SeasonCommandHandler commandHandler, IQeryRepository qeryRepository)
        {
            _commandHandler = commandHandler;
            _qeryRepository = qeryRepository;
        }

        [HttpGet]
        public async Task<ActionResult> GetSeasons()
        {
            var seasons = await _qeryRepository.Load<AllSeasonsQuery>();
            return Ok(seasons.Value);
        }

        [HttpGet("{entityId}")]
        public async Task<ActionResult> GetSeason(Guid entityId)
        {
            var seasons = await _qeryRepository.Load<SingleSeasonsQuery>(entityId);
            return Ok(seasons.Value);
        }

        [HttpGet("Counter")]
        public async Task<ActionResult> GetSeasonsCount()
        {
            var counter = await _qeryRepository.Load<AllSeasonsCounterQuery>();
            return Ok(counter.Value);
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