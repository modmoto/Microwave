using System;
using System.Collections.Generic;

namespace Microwave.Application.Discovery
{
    public interface IEventLocation
    {
        SubscriberEventAndReadmodelConfig GetServiceForEvent(Type eventType);
        SubscriberEventAndReadmodelConfig GetServiceForReadModel(Type eventType);
        void SetDomainEventLocation(SubscriberEventAndReadmodelConfig service);
        IEnumerable<SubscriberEventAndReadmodelConfig> Services { get; }
    }
}