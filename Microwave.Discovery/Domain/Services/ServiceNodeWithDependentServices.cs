using System.Collections.Generic;

namespace Microwave.Discovery.Domain.Services
{
    public class ServiceNodeWithDependentServices
    {
        public static ServiceNodeWithDependentServices Reachable(ServiceEndPoint serviceEndPoint, IEnumerable<ServiceNode> services)
        {
            return new ServiceNodeWithDependentServices(serviceEndPoint, true, services);
        }

        public static ServiceNodeWithDependentServices NotReachable(ServiceEndPoint serviceEndPoint)
        {
            return new ServiceNodeWithDependentServices(serviceEndPoint, true, new List<ServiceNode>());
        }

        private ServiceNodeWithDependentServices(ServiceEndPoint serviceEndPoint, bool isReachable, IEnumerable<ServiceNode> services)
        {
            ServiceEndPoint = serviceEndPoint;
            IsReachable = isReachable;
            Services = services;
        }

        public ServiceEndPoint ServiceEndPoint { get; }
        public bool IsReachable { get; }
        public IEnumerable<ServiceNode> Services { get; }
    }
}