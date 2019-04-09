using System.Collections.Generic;

namespace Microwave.Discovery.Domain.Services
{
    public class ServiceNode
    {
        public static ServiceNode Reachable(ServiceEndPoint serviceEndPoint, IEnumerable<MicrowaveService> services)
        {
            return new ServiceNode(serviceEndPoint, true, services);
        }

        public static ServiceNode NotReachable(ServiceEndPoint serviceEndPoint)
        {
            return new ServiceNode(serviceEndPoint, true, new List<MicrowaveService>());
        }

        private ServiceNode(ServiceEndPoint serviceEndPoint, bool isReachable, IEnumerable<MicrowaveService> services)
        {
            ServiceEndPoint = serviceEndPoint;
            IsReachable = isReachable;
            Services = services;
        }

        public ServiceEndPoint ServiceEndPoint { get; }
        public bool IsReachable { get; }
        public IEnumerable<MicrowaveService> Services { get; }
    }
}