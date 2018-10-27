using System;
using System.Threading.Tasks;
using Application.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Adapters.Framework.WebApi
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
        public async Task<ActionResult> GetDomainEventsByType(string eventType, [FromQuery] long myLastVersion = -1)
        {
            var result = await _eventRepository.LoadEventsByTypeAsync(eventType, myLastVersion);
            return Ok(result.Value);
        }

        [HttpGet("EntityStreams/{entityId}")]
        public async Task<ActionResult> GetDomainEventsByEntityIdType(Guid entityId, [FromQuery] long myLastVersion = -1)
        {
            var result = await _eventRepository.LoadEventsByEntity(entityId, myLastVersion);
            return Ok(result.Value);
        }
    }
}