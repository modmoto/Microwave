using System.Collections.Generic;
using Microwave.Discovery.Domain.Services;

namespace Microwave.Discovery.Domain.Events
{
    public class EventsPublishedByService
    {
        public EventsPublishedByService(ServiceEndPoint serviceEndPoint,
            IEnumerable<EventSchema> publishedEventTypes,
            bool isReachable = true)
        {
            ServiceEndPoint = serviceEndPoint;
            PublishedEventTypes = publishedEventTypes;
            IsReachable = isReachable;
        }

        public ServiceEndPoint ServiceEndPoint { get; }
        public IEnumerable<EventSchema> PublishedEventTypes { get; }
        public bool IsReachable { get; }
    }

}