using System;
using System.Collections.Generic;
using Microwave.Discovery.Domain.Events;
using Microwave.Discovery.Domain.Services;

namespace Microwave.Discovery.Domain
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