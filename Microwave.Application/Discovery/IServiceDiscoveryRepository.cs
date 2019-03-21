using System;
using System.Threading.Tasks;

namespace Microwave.Application.Discovery
{
    public interface IServiceDiscoveryRepository
    {
        Task<PublisherEventConfig> GetPublishedEventTypes(Uri serviceAdress);
    }
}