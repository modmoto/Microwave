using System.Collections.Generic;
using Microwave.Discovery.EventLocations;

namespace Microwave.WebApi.Discovery
{
    internal class PublishedEventsByServiceDto
    {
        public string ServiceName { get; set; }
        public List<EventSchema> PublishedEvents { get; } = new List<EventSchema>();
    }
}