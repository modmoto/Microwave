using System.Collections.Generic;
using Microwave.Discovery.Domain.Events;
using Microwave.Discovery.Domain.Services;

namespace Microwave.Discovery.Domain
{
    public class EventLocationDto
    {
        public string ServiceName { get; }
        public IEnumerable<ServiceNode> Services { get; }
        public IEnumerable<EventSchema> UnresolvedEventSubscriptions { get; }
        public IEnumerable<ReadModelSubscription> UnresolvedReadModeSubscriptions { get; }

        public EventLocationDto(
            IEnumerable<ServiceNode> services,
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