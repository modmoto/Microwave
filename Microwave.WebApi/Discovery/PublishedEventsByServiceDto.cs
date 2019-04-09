using System.Collections.Generic;
using Microwave.Discovery;

namespace Microwave.WebApi.Discovery
{
    public class PublishedEventsByServiceDto
    {
        public string ServiceName { get; set; }
        public List<EventSchema> PublishedEvents { get; } = new List<EventSchema>();
    }
}