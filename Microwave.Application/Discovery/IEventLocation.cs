using System;
using System.Collections.Generic;

namespace Microwave.Application.Discovery
{
    public interface IEventLocation
    {
        SubscriberEventAndReadmodelConfig GetServiceForEvent(Type eventType);
        SubscriberEventAndReadmodelConfig GetServiceForReadModel(Type readModel);
        void SetDomainEventLocation(SubscriberEventAndReadmodelConfig service);
        IEnumerable<SubscriberEventAndReadmodelConfig> Services { get; }

        IEnumerable<string> UnresolvedEventSubscriptions { get; }
        IEnumerable<ReadModelSubscription> UnresolvedReadModeSubscriptions { get; }
    }
}