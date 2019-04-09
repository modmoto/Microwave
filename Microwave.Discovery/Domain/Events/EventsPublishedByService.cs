using System.Collections.Generic;
using Microwave.Discovery.Domain.Services;

namespace Microwave.Discovery.Domain.Events
{
    public class EventsPublishedByService
    {
        public static EventsPublishedByService Reachable(
            ServiceEndPoint serviceEndPoint,
            IEnumerable<EventSchema> publishedEventTypes)
        {
            return new EventsPublishedByService(serviceEndPoint, publishedEventTypes, true);
        }

        public static EventsPublishedByService NotReachable(
            ServiceEndPoint serviceEndPoint)
        {
            return new EventsPublishedByService(serviceEndPoint, new List<EventSchema>(), false);
        }

        private EventsPublishedByService(ServiceEndPoint serviceEndPoint,
            IEnumerable<EventSchema> publishedEventTypes,
            bool isReachable)
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