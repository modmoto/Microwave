using System.Threading.Tasks;

namespace Microwave.Application.Discovery
{
    public interface IStatusRepository
    {
        Task SaveEventLocation(EventLocation eventLocation);
        Task<IEventLocation> GetEventLocation();
    }
}