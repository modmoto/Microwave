using System;
using System.Threading.Tasks;

namespace Microwave.Application.Discovery
{
    public interface IServiceDiscoveryRepository
    {
        Task<ConsumingService> GetPublishedEventTypes(Uri serviceAdress);
    }
}