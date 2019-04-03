using System;
using System.Collections.Generic;

namespace Microwave.Application.Discovery
{
    public interface IEventLocation
    {
        SubscriberEventAndReadmodelConfig GetServiceForEvent(Type eventType);
        SubscriberEventAndReadmodelConfig GetServiceForReadModel(Type readModel);
        IEnumerable<SubscriberEventAndReadmodelConfig> Services { get; }

        IEnumerable<EventSchema> UnresolvedEventSubscriptions { get; }
        IEnumerable<ReadModelSubscription> UnresolvedReadModeSubscriptions { get; }
    }
}