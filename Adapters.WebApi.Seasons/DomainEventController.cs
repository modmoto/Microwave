using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading.Tasks;
using Application.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Adapters.WebApi.Seasons
{
    [Route("Api")]
    public class DomainEventController : Controller
    {
        private readonly IEventRepository _eventRepository;

        public DomainEventController(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        [HttpGet("DomainEventTypeStreams")]
        public async Task<ActionResult> GetDomainEventsByType([FromQuery] string eventType, [FromQuery] long myLastVersion = -1)
        {
            return Ok(await _eventRepository.LoadEventsByTypeAsync(eventType, myLastVersion));
        }

        [HttpGet("EntityStreams/{entityId}")]
        public async Task<ActionResult> GetDomainEventsByEntityIdType(Guid entityId, [FromQuery] long myLastVersion = -1)
        {
            return Ok(await _eventRepository.LoadEventsByEntity(entityId, myLastVersion));
        }
    }
}