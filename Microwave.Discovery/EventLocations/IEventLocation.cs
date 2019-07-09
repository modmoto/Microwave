using System;
using System.Collections.Generic;
using Microwave.Discovery.ServiceMaps;

namespace Microwave.Discovery.EventLocations
{
    public interface IEventLocation
    {
        MicrowaveServiceNode GetServiceForEvent(Type eventType);
        MicrowaveServiceNode GetServiceForReadModel(Type readModel);
        IEnumerable<MicrowaveServiceNode> Services { get; }
        IEnumerable<EventSchema> UnresolvedEventSubscriptions { get; }
        IEnumerable<ReadModelSubscription> UnresolvedReadModeSubscriptions { get; }
    }
}