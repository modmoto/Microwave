using Microsoft.AspNetCore.Mvc;
using Microwave.Application;

namespace Microwave.WebApi
{
    [Route("Dicovery")]
    public class DiscoveryController : Controller
    {
        private readonly PublishedEventCollection _publishedEvents;

        public DiscoveryController(PublishedEventCollection publishedEvents)
        {
            _publishedEvents = publishedEvents;
        }

        [HttpGet("PublishedEvents")]
        public ActionResult GetPublishedEvents()
        {
            return Ok(_publishedEvents);
        }
    }
}