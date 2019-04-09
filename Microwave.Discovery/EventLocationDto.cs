using System.Collections.Generic;

namespace Microwave.Discovery
{
    public class EventLocationDto
    {
        public string ServiceName { get; }
        public IEnumerable<MicrowaveService> Services { get; }
        public IEnumerable<EventSchema> UnresolvedEventSubscriptions { get; }
        public IEnumerable<ReadModelSubscription> UnresolvedReadModeSubscriptions { get; }

        public EventLocationDto(
            IEnumerable<MicrowaveService> services,
            IEnumerable<EventSchema> unresolvedEventSubscriptions,
            IEnumerable<ReadModelSubscription> unresolvedReadModeSubscriptions,
            string serviceName)
        {
            Services = services;
            UnresolvedEventSubscriptions = unresolvedEventSubscriptions;
            UnresolvedReadModeSubscriptions = unresolvedReadModeSubscriptions;
            ServiceName = serviceName;
        }
    }
}