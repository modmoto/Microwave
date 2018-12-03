using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.Application.Ports;

namespace Microwave.WebApi
{
    [Route("Api")]
    public class DomainEventController : Controller
    {
        private readonly IEntityStreamRepository _entityStreamRepository;
        private readonly ITypeProjectionRepository _typeProjectionRepository;

        public DomainEventController(
            IEntityStreamRepository entityStreamRepository,
            ITypeProjectionRepository typeProjectionRepository)
        {
            _entityStreamRepository = entityStreamRepository;
            _typeProjectionRepository = typeProjectionRepository;
        }

        [HttpGet("DomainEventTypeStreams/{eventType}")]
        public async Task<ActionResult> GetDomainEventsByType(string eventType, [FromQuery] long version = 0)
        {
            var result = await _typeProjectionRepository.LoadEventsByTypeAsync(eventType, version);
            return Ok(result.Value);
        }

        [HttpGet("EntityStreams/{entityId}")]
        public async Task<ActionResult> GetDomainEventsByEntityIdType(Guid entityId, [FromQuery] long version = 0)
        {
            var result = await _entityStreamRepository.LoadEventsByEntity(entityId, version);
            return Ok(result.Value);
        }

        [HttpGet("DomainEvents")]
        public async Task<ActionResult> GetAllDomainEvents([FromQuery] long createdSince = 0)
        {
            var result = await _entityStreamRepository.LoadEvents(createdSince);
            return Ok(result.Value);
        }
    }
}