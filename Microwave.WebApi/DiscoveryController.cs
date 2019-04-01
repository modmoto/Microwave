using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.Application;
using Microwave.Application.Discovery;

namespace Microwave.WebApi
{
    [Route("Dicovery")]
    public class DiscoveryController : Controller
    {
        private readonly ServicePublishingConfig _publishedEvents;
        private readonly DiscoveryHandler _discoveryHandler;

        public DiscoveryController(ServicePublishingConfig publishedEvents, DiscoveryHandler discoveryHandler)
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
        public ActionResult GetConsumingServices()
        {
            return Ok(_discoveryHandler.GetConsumingServices());
        }

        [HttpPut("ConsumingServices/Update")]
        public async Task<ActionResult> UpdateConsumingServices()
        {
            await _discoveryHandler.DiscoverConsumingServices();
            return Ok();
        }
    }
}