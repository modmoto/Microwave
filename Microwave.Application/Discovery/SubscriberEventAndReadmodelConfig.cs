using System;
using System.Collections.Generic;

namespace Microwave.Application.Discovery
{
    public class SubscriberEventAndReadmodelConfig
    {
        public Uri ServiceBaseAddress { get; }
        public IEnumerable<string> SubscribedEvents { get; }
        public IEnumerable<ReadModelSubscription> ReadModels { get; }
        public string ServiceName { get; }

        public SubscriberEventAndReadmodelConfig(
            Uri serviceBaseAddress,
            IEnumerable<string> subscribedEvents,
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