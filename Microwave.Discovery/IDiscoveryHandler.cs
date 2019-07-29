using System;
using System.Threading.Tasks;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using Microwave.WebApi.Discovery;

namespace Microwave.Discovery
{
    public interface IDiscoveryHandler
    {
        Task<EventLocation> GetConsumingServices();
        Task<ServiceMap> GetServiceMap();
        Task DiscoverConsumingServices();
        Task DiscoverServiceMap();
        Task<MicrowaveServiceNode> GetConsumingServiceNodes();
        Task<PublishedEventsByServiceDto> GetPublishedEvents();
    }
}