using System;
using System.Collections.Generic;
using System.Linq;
using Microwave.Application.Discovery;

namespace Microwave.WebApi
{
    public class EventLocation
    {
        private IEnumerable<ConsumingService> _services = new List<ConsumingService>();

        public Uri GetDomainEventLocation(Type eventType)
        {
            return _services.FirstOrDefault(s => s.PublishedEventTypes.Contains(eventType.Name))?.ServiceBaseAddress;
        }

        public void SetDomainEventLocation(ConsumingService service)
        {
            _services = _services.Append(service);
        }
    }
}