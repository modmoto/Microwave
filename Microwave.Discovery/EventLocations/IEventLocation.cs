using System;
using System.Collections.Generic;
using Microwave.Discovery.ServiceMaps;

namespace Microwave.Discovery.EventLocations
{
    public interface IEventLocation
    {
        ServiceNode GetServiceForEvent(Type eventType);
        ServiceNode GetServiceForReadModel(Type readModel);
        IEnumerable<ServiceNode> Services { get; }
        IEnumerable<EventSchema> UnresolvedEventSubscriptions { get; }
        IEnumerable<ReadModelSubscription> UnresolvedReadModeSubscriptions { get; }
    }
}