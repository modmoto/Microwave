using System.Threading.Tasks;

namespace Microwave.Discovery
{
    public interface IStatusRepository
    {
        Task SaveEventLocation(EventLocation eventLocation);
        Task<IEventLocation> GetEventLocation();
        Task<ServiceMap> GetServiceMap();
    }
}