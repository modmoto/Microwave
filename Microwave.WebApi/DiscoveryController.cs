using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.Discovery;

namespace Microwave.WebApi
{
    [Route("Dicovery")]
    public class DiscoveryController : Controller
    {
        private readonly PublishedEventsByServiceDto _publishedEvents;
        private readonly DiscoveryHandler _discoveryHandler;

        public DiscoveryController(PublishedEventsByServiceDto publishedEvents, DiscoveryHandler discoveryHandler)
        {
            _publishedEvents = publishedEvents;
            _discoveryHandler = discoveryHandler;
        }

        [HttpGet("PublishedEvents")]
        public ActionResult GetPublishedEvents()
        {
            return Ok(_publishedEvents);
        }

        [HttpGet("ConsumingServices")]
        public async Task<ActionResult> GetConsumingServices()
        {
            var consumingServices = await _discoveryHandler.GetConsumingServices();
            return Ok(consumingServices);
        }

        [HttpPut("ConsumingServices/Update")]
        public async Task<ActionResult> UpdateConsumingServices()
        {
            await _discoveryHandler.DiscoverConsumingServices();
            return Ok();
        }
    }
}