using System;
using System.Threading.Tasks;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;

namespace Microwave.Discovery
{
    public interface IServiceDiscoveryRepository
    {
        Task<EventsPublishedByService> GetPublishedEventTypes(Uri serviceAdress);
        Task<ServiceNodeConfig> GetDependantServices(Uri serviceAddress);
    }
}