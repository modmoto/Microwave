using System;
using System.Collections.Generic;

namespace Microwave.Discovery
{
    public class MicrowaveService
    {
        public Uri ServiceBaseAddress => NodeEntryPoint.ServiceBaseAddress;
        public NodeEntryPoint NodeEntryPoint { get; }
        public IEnumerable<EventSchema> SubscribedEvents { get; }
        public IEnumerable<ReadModelSubscription> ReadModels { get; }
        public string ServiceName => NodeEntryPoint.Name;

        public MicrowaveService(
            NodeEntryPoint nodeEntryPoint,
            IEnumerable<EventSchema> subscribedEvents,
            IEnumerable<ReadModelSubscription> readModels)
        {
            NodeEntryPoint = nodeEntryPoint;
            SubscribedEvents = subscribedEvents;
            ReadModels = readModels;
        }
    }
}