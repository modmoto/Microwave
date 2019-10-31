using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.EventStores.Ports;

namespace Microwave.WebApi.Queries
{
    [Route("Api")]
    public class DomainEventController : Controller
    {
        private readonly IEventRepository _eventRepository;

        public DomainEventController(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        [HttpGet("DomainEventTypeStreams/{eventType}")]
        public async Task<ActionResult> GetDomainEventsByType(string eventType, [FromQuery] long lastVersion)
        {
            var result = await _eventRepository.LoadEventsByTypeAsync(eventType, lastVersion);
            return Ok(result.Value);
        }

        [HttpGet("EntityStreams/{entityId}")]
        public async Task<ActionResult> GetDomainEventsByEntityIdType(string entityId, [FromQuery] long lastVersion)
        {
            var result = await _eventRepository.LoadEventsByEntity(entityId, lastVersion);
            return Ok(result.Value);
        }

        [HttpGet("DomainEvents")]
        public async Task<ActionResult> GetAllDomainEvents([FromQuery] long lastVersion)
        {
            var result = await _eventRepository.LoadEvents(lastVersion);
            return Ok(result.Value);
        }
    }
}