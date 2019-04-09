using System;
using System.Threading.Tasks;
using Microwave.Discovery.Domain.Events;
using Microwave.Discovery.Domain.Services;

namespace Microwave.Discovery
{
    public interface IServiceDiscoveryRepository
    {
        Task<EventsPublishedByService> GetPublishedEventTypes(Uri serviceAdress);
        Task<ServiceNodeWithDependentServices> GetDependantServices(Uri serviceAdress);
    }
}