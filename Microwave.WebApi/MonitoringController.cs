using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.Application.Monitoring;
using Microwave.EventStores.Ports;

namespace Microwave.WebApi
{
    [Route("Monitoring")]
    public class MonitoringController : Controller
    {
        private readonly MonitoringHandler _eventRepository;

        public MonitoringController(MonitoringHandler eventRepository)
        {
            _eventRepository = eventRepository;
        }

        [HttpGet("EventStreamVersions/{eventType}")]
        public async Task<ActionResult> GetStreamVersion(string eventType)
        {
            var streamVersion = await _eventRepository.GetEventTypeCount(eventType);
            return Ok(new StreamVersion(eventType, streamVersion));
        }
    }
}