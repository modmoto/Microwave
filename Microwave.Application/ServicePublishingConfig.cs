using System.Collections.Generic;
using Microwave.Application.Discovery;

namespace Microwave.Application
{
    public class ServicePublishingConfig
    {
        public string ServiceName { get; set; }
        public List<EventSchema> PublishedEvents { get; } = new List<EventSchema>();
    }
}