using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.Application;

namespace Adapters.Framework.WebApi
{
    [Route("Api")]
    public class DomainEventController : Controller
    {
        private readonly IEntityStreamRepository _entityStreamRepository;
        private readonly ITypeProjectionRepository _typeProjectionRepository;
        private readonly IOverallProjectionRepository _overallProjectionRepository;

        public DomainEventController(
            IEntityStreamRepository entityStreamRepository,
            ITypeProjectionRepository typeProjectionRepository,
            IOverallProjectionRepository overallProjectionRepository)
        {
            _entityStreamRepository = entityStreamRepository;
            _typeProjectionRepository = typeProjectionRepository;
            _overallProjectionRepository = overallProjectionRepository;
        }

        [HttpGet("DomainEventTypeStreams/{eventType}")]
        public async Task<ActionResult> GetDomainEventsByType(string eventType, [FromQuery] long myLastVersion = -1)
        {
            var result = await _typeProjectionRepository.LoadEventsByTypeAsync(eventType, myLastVersion);
            return Ok(result.Value);
        }

        [HttpGet("EntityStreams/{entityId}")]
        public async Task<ActionResult> GetDomainEventsByEntityIdType(Guid entityId, [FromQuery] long myLastVersion = -1)
        {
            var result = await _entityStreamRepository.LoadEventsByEntity(entityId, myLastVersion);
            return Ok(result.Value);
        }

        [HttpGet("DomainEvents")]
        public async Task<ActionResult> GetAllDomainEvents([FromQuery] long myLastVersion = -1)
        {
            var result = await _overallProjectionRepository.LoadOverallStream(myLastVersion);
            return Ok(result.Value);
        }
    }
}