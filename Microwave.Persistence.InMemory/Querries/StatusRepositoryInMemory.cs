using System;
using System.Threading.Tasks;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;

namespace Microwave.Persistence.InMemory.Querries
{
    public class StatusRepositoryInMemory : IStatusRepository
    {
        public Task SaveEventLocation(EventLocation eventLocation)
        {
            throw new NotImplementedException();
        }

        public Task<EventLocation> GetEventLocation()
        {
            throw new NotImplementedException();
        }

        public Task<ServiceMap> GetServiceMap()
        {
            throw new NotImplementedException();
        }

        public Task SaveServiceMap(ServiceMap map)
        {
            throw new NotImplementedException();
        }
    }
}