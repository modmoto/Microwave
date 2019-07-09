using System.Threading.Tasks;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;

namespace Microwave.Discovery
{
    public interface IDiscoveryHandler
    {
        Task<EventLocationDto> GetConsumingServices();
        Task<ServiceMap> GetServiceMap();
        Task DiscoverConsumingServices();
        Task DiscoverServiceMap();
        Task<MicrowaveServiceNode> GetConsumingServiceNodes();
    }
}