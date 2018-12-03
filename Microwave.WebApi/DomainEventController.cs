using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.Application.Ports;

namespace Microwave.WebApi
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
        public async Task<ActionResult> GetDomainEventsByType(string eventType, [FromQuery] long timeStamp)
        {
            var result = await _eventRepository.LoadEventsByTypeAsync(eventType, timeStamp);
            return Ok(result.Value);
        }

        [HttpGet("EntityStreams/{entityId}")]
        public async Task<ActionResult> GetDomainEventsByEntityIdType(Guid entityId, [FromQuery] long version)
        {
            var result = await _eventRepository.LoadEventsByEntity(entityId, version);
            return Ok(result.Value);
        }

        [HttpGet("DomainEvents")]
        public async Task<ActionResult> GetAllDomainEvents([FromQuery] long timeStamp)
        {
            var result = await _eventRepository.LoadEvents(timeStamp);
            return Ok(result.Value);
        }
    }
}