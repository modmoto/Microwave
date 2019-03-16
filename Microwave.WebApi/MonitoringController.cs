using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.EventStores.Ports;
using Microwave.Queries;

namespace Microwave.WebApi
{
    [Route("Monitoring")]
    public class MonitoringController : Controller
    {
        private readonly IEventRepository _eventRepository;
        private readonly IVersionRepository _versionRepository;

        public MonitoringController(IEventRepository eventRepository, IVersionRepository versionRepository)
        {
            _eventRepository = eventRepository;
            _versionRepository = versionRepository;
        }

        [HttpGet("EventStreamVersions/{eventType}")]
        public async Task<ActionResult> GetStreamVersion(string eventType)
        {
            var streamVersion = await _eventRepository.GetEventTypeCount(eventType);
            return Ok(new StreamVersion
            {
                Version = streamVersion.Value,
                DomainEventType = eventType
            });
        }
    }
}