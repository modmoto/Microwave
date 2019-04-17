using System.Threading.Tasks;
using Microwave.Discovery.Domain;
using Microwave.Discovery.Domain.Services;

namespace Microwave.Discovery
{
    public interface IDiscoveryHandler
    {
        Task<EventLocationDto> GetConsumingServices();
        Task<ServiceMap> GetServiceMap();
        Task DiscoverConsumingServices();
        Task DiscoverServiceMap();
        Task<ServiceNodeWithDependentServicesDto> GetConsumingServiceNodes();
    }
}