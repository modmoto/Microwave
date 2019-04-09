using System.Collections.Generic;
using Microwave.Discovery.Domain.Events;

namespace Microwave.Discovery.Domain.Services
{
    public class ServiceNode
    {
        public ServiceEndPoint ServiceEndPoint { get; }
        public IEnumerable<EventSchema> SubscribedEvents { get; }
        public IEnumerable<ReadModelSubscription> ReadModels { get; }

        public ServiceNode(
            ServiceEndPoint serviceEndPoint,
            IEnumerable<EventSchema> subscribedEvents,
            IEnumerable<ReadModelSubscription> readModels)
        {
            ServiceEndPoint = serviceEndPoint;
            SubscribedEvents = subscribedEvents;
            ReadModels = readModels;
        }
    }
}