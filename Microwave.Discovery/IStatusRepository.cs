using System.Threading.Tasks;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;

namespace Microwave.Discovery
{
    internal interface IStatusRepository
    {
        Task SaveEventLocation(EventLocation eventLocation);
        Task<EventLocation> GetEventLocation();
        Task<ServiceMap> GetServiceMap();
        Task SaveServiceMap(ServiceMap map);
    }
}