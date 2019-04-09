using System.Threading.Tasks;
using Microwave.Discovery.Domain;

namespace Microwave.Discovery
{
    public interface IStatusRepository
    {
        Task SaveEventLocation(EventLocation eventLocation);
        Task<IEventLocation> GetEventLocation();
        Task<ServiceMap> GetServiceMap();
    }
}