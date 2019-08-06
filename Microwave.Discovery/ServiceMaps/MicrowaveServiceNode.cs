using System.Collections.Generic;
using Microwave.Discovery.EventLocations;

namespace Microwave.Discovery.ServiceMaps
{
    public class MicrowaveServiceNode
    {
        public static MicrowaveServiceNode Create(
            ServiceEndPoint serviceEndPoint,
            IEnumerable<EventSchema> subscribedEvents,
            IEnumerable<ReadModelSubscription> readModels)
        {
            return new MicrowaveServiceNode(serviceEndPoint, subscribedEvents, readModels);
        }

        public static MicrowaveServiceNode ReachableMicrowaveServiceNode(
            ServiceEndPoint serviceEndPoint,
            IEnumerable<ServiceEndPoint> connectedServices)
        {
            return new MicrowaveServiceNode(serviceEndPoint, connectedServices, true);
        }

        public static MicrowaveServiceNode UnreachableMicrowaveServiceNode(
            ServiceEndPoint serviceEndPoint,
            IEnumerable<ServiceEndPoint> connectedServices)
        {
            return new MicrowaveServiceNode(serviceEndPoint, connectedServices, false);
        }

        private MicrowaveServiceNode(
            ServiceEndPoint serviceEndPoint,
            IEnumerable<EventSchema> subscribedEvents,
            IEnumerable<ReadModelSubscription> readModels)
        {
            ServiceEndPoint = serviceEndPoint;
            ConnectedServices = new List<ServiceEndPoint>();
            IsReachable = true;
            SubscribedEvents = subscribedEvents;
            ReadModels = readModels;
        }

        private MicrowaveServiceNode(
            ServiceEndPoint serviceEndPoint,
            IEnumerable<ServiceEndPoint> connectedServices,
            bool isReachable)
        {
            ServiceEndPoint = serviceEndPoint;
            ConnectedServices = connectedServices;
            IsReachable = isReachable;
            SubscribedEvents = new List<EventSchema>();
            ReadModels = new List<ReadModelSubscription>();
        }

        public bool IsReachable { get; }
        public ServiceEndPoint ServiceEndPoint { get; }
        public IEnumerable<ServiceEndPoint> ConnectedServices { get; }
        public IEnumerable<EventSchema> SubscribedEvents { get; }
        public IEnumerable<ReadModelSubscription> ReadModels { get; }
    }
}