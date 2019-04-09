using System;
using System.Collections.Generic;

namespace Microwave.Discovery
{
    public class EventsPublishedByService
    {
        public EventsPublishedByService(NodeEntryPoint nodeEntryPoint,
            IEnumerable<EventSchema> publishedEventTypes,
            bool isReachable = true)
        {
            NodeEntryPoint = nodeEntryPoint;
            PublishedEventTypes = publishedEventTypes;
            IsReachable = isReachable;
        }

        public NodeEntryPoint NodeEntryPoint { get; }
        public IEnumerable<EventSchema> PublishedEventTypes { get; }
        public bool IsReachable { get; }
        public Uri ServiceBaseAddress => NodeEntryPoint.ServiceBaseAddress;
        public string ServiceName => NodeEntryPoint.Name;
    }

}