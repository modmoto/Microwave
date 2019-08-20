using System;
using System.Threading.Tasks;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;

namespace Microwave.Discovery
{
    public interface IDiscoveryHandler
    {
        Task<EventLocation> GetConsumingServices();
        Task<ServiceMap> GetServiceMap();
        Task DiscoverConsumingServices();
        Task DiscoverServiceMap();
        Task<MicrowaveServiceNode> GetConsumingServiceNodes();
        Task<EventsPublishedByService> GetPublishedEvents();
        Task SubscribeOnDiscoveredServices();
        Task SubscribeForEvent(EventSchema schema, Uri requestOrigin);
    }
}