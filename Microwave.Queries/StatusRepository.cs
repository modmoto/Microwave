using System.Threading.Tasks;
using Microwave.Application.Discovery;

namespace Microwave.Queries
{
    public class StatusRepository : IStatusRepository
    {
        public Task SaveEventLocation(EventLocation eventLocation)
        {
            throw new System.NotImplementedException();
        }

        public Task<IEventLocation> GetEventLocation()
        {
            throw new System.NotImplementedException();
        }
    }
}