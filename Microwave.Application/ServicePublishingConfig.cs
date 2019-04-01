using System.Collections.Generic;

namespace Microwave.Application
{
    public class ServicePublishingConfig
    {
        public string ServiceName { get; set; }
        public List<string> PublishedEvents { get; } = new List<string>();
    }
}