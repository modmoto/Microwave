using System;
using System.Threading.Tasks;
using Application.Framework;
using Microsoft.AspNetCore.Mvc;

namespace Adapters.WebApi.Seasons
{
    [Route("Api/DomainEvents")]
    public class DomainEventController : Controller
    {
        private readonly IEventRepository _eventRepository;

        public DomainEventController(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        [HttpGet]
        public async Task<ActionResult> GetDomainEventsByType([FromQuery] string eventType, [FromQuery] Guid entityId, [FromQuery] long myLastVersion = -1)
        {
            if (!string.IsNullOrEmpty(eventType))
            {
                return Ok(await _eventRepository.LoadEventsByType(eventType, myLastVersion));
            }

            return Ok(await _eventRepository.LoadEventsByEntity(entityId, myLastVersion));
        }
    }
}