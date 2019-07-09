using System;
using System.Collections.Generic;
using Microwave.Discovery.EventLocations;
using Newtonsoft.Json;

namespace Microwave.Discovery.ServiceMaps
{
    public class MicrowaveServiceNode
    {
        public MicrowaveServiceNode(
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

        [JsonConstructor]
        public MicrowaveServiceNode(ServiceEndPoint serviceEndPoint,
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
        public ServiceEndPoint ServiceEndPoint { get; private set; }
        public IEnumerable<ServiceEndPoint> ConnectedServices { get; }
        public IEnumerable<EventSchema> SubscribedEvents { get; }
        public IEnumerable<ReadModelSubscription> ReadModels { get; }

        public void SetAddressForEndPoint(Uri serviceAddress)
        {
            ServiceEndPoint = new ServiceEndPoint(serviceAddress, ServiceEndPoint.Name);
        }
    }
}