using System.Threading.Tasks;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;

namespace Microwave.Persistence.InMemory.Querries
{
    public class StatusRepositoryInMemory : IStatusRepository
    {
        private ServiceMap _map;
        private EventLocation _location;

        public Task SaveEventLocation(EventLocation eventLocation)
        {
            _location = eventLocation;
            return Task.CompletedTask;
        }

        public Task<EventLocation> GetEventLocation()
        {
            return Task.FromResult(_location ?? EventLocation.Default());
        }

        public Task<ServiceMap> GetServiceMap()
        {
            return Task.FromResult(_map);
        }

        public Task SaveServiceMap(ServiceMap map)
        {
            _map = map;
            return Task.CompletedTask;
        }
    }
}