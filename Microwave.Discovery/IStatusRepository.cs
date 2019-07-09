using System.Threading.Tasks;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;

namespace Microwave.Discovery
{
    public interface IStatusRepository
    {
        Task SaveEventLocation(IEventLocation eventLocation);
        Task<IEventLocation> GetEventLocation();
        Task<ServiceMap> GetServiceMap();
        Task SaveServiceMap(ServiceMap map);
    }
}