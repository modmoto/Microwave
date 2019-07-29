using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microwave.Discovery;

namespace Microwave.WebApi.Discovery
{
    [Route("Dicovery")]
    public class DiscoveryController : Controller
    {
        private readonly IDiscoveryHandler _discoveryHandler;

        public DiscoveryController(IDiscoveryHandler discoveryHandler)
        {
            _discoveryHandler = discoveryHandler;
        }

        [HttpGet("PublishedEvents")]
        public async Task<ActionResult> GetPublishedEvents()
        {
            var publishedEvents = await _discoveryHandler.GetPublishedEvents();
            return Ok(publishedEvents);
        }

        [HttpGet("ConsumingServices")]
        public async Task<ActionResult> GetConsumingServices()
        {
            var consumingServices = await _discoveryHandler.GetConsumingServices();
            return Ok(consumingServices);
        }

        [HttpGet("ServiceDependencies")]
        public async Task<ActionResult> GetServiceDependencies()
        {
            var consumingServices = await _discoveryHandler.GetConsumingServiceNodes();
            return Ok(consumingServices);
        }

        [HttpPut("ServiceMap/Update")]
        public async Task<ActionResult> UpdateServiceMap()
        {
            await _discoveryHandler.DiscoverServiceMap();
            return Ok();
        }

        [HttpPut("ServiceMap")]
        public async Task<ActionResult> GetServiceMap()
        {
            var serviceMap = await _discoveryHandler.GetServiceMap();
            return Ok(serviceMap);
        }

        [HttpPut("ConsumingServices/Update")]
        public async Task<ActionResult> UpdateConsumingServices()
        {
            await _discoveryHandler.DiscoverConsumingServices();
            return Ok();
        }
    }
}