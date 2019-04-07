using System;
using System.Collections.Generic;

namespace Microwave.Application.Discovery
{
    public interface IEventLocation
    {
        MicrowaveService GetServiceForEvent(Type eventType);
        MicrowaveService GetServiceForReadModel(Type readModel);
        IEnumerable<MicrowaveService> Services { get; }
        IEnumerable<EventSchema> UnresolvedEventSubscriptions { get; }
        IEnumerable<ReadModelSubscription> UnresolvedReadModeSubscriptions { get; }
    }
}