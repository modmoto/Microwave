using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microwave.Application.Discovery
{
    public interface IServiceDiscoveryRepository
    {
        Task<EventsPublishedByService> GetPublishedEventTypes(Uri serviceAdress);
        Task<ServiceNode> GetDependantServices(Uri serviceAdress);
    }

    public class ServiceNode
    {
        public string NodeName { get; set; }
        public Uri ServiceBaseAddress { get; set; }
        public bool IsReachable { get; set; }
        public IEnumerable<MicrowaveService> Services { get; set; }
    }
}