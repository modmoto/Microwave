using System;
using System.Threading.Tasks;
using Application.Framework;
using Application.Seasons;
using Application.Seasons.Commands;
using Application.Seasons.Querries;
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
        public async Task<ActionResult> GetDomainEventsByType(string eventType, long from = -1)
        {
            var events = await _eventRepository.LoadEventsByType(eventType, from);
            return Ok(events);
        }

        [HttpGet]
        public async Task<ActionResult> GetDomainEventsByEntity(Guid entityId, long from = -1)
        {
            var events = await _eventRepository.LoadEventsByEntity(entityId, from);
            return Ok(events);
        }
    }
}