using System;
using System.Collections.Generic;
using Microwave.Discovery.Domain.Events;
using Microwave.Discovery.Domain.Services;

namespace Microwave.Discovery.Domain
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