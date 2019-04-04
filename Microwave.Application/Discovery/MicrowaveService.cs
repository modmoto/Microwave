using System;
using System.Collections.Generic;

namespace Microwave.Application.Discovery
{
    public class MicrowaveService
    {
        public Uri ServiceBaseAddress { get; }
        public IEnumerable<EventSchema> SubscribedEvents { get; }
        public IEnumerable<ReadModelSubscription> ReadModels { get; }
        public string ServiceName { get; }

        public MicrowaveService(
            Uri serviceBaseAddress,
            IEnumerable<EventSchema> subscribedEvents,
            IEnumerable<ReadModelSubscription> readModels,
            string serviceName = null)
        {
            ServiceBaseAddress = serviceBaseAddress;
            SubscribedEvents = subscribedEvents;
            ReadModels = readModels;
            ServiceName = serviceName ?? serviceBaseAddress.ToString();
        }
    }
}