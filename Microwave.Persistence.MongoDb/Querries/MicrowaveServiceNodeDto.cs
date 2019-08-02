using System.Collections.Generic;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;

namespace Microwave.Persistence.MongoDb.Querries
{
    public class MicrowaveServiceNodeDto
    {
        public bool IsReachable { get; set; }
        public ServiceEndPoint ServiceEndPoint { get; set; }
        public IEnumerable<ServiceEndPoint> ConnectedServices { get; set; }
        public IEnumerable<EventSchema> SubscribedEvents { get; set; }
        public IEnumerable<ReadModelSubscription> ReadModels { get; set; }
    }
}