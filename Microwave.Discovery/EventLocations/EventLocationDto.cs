using System.Collections.Generic;
using Microwave.Discovery.ServiceMaps;

namespace Microwave.Discovery.EventLocations
{
    public class EventLocationDto
    {
        public string ServiceName { get; }
        public IEnumerable<MicrowaveServiceNode> Services { get; }
        public IEnumerable<EventSchema> UnresolvedEventSubscriptions { get; }
        public IEnumerable<ReadModelSubscription> UnresolvedReadModeSubscriptions { get; }

        public EventLocationDto(
            IEnumerable<MicrowaveServiceNode> services,
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