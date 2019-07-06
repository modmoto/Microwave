using System.Collections.Generic;

namespace Microwave.Discovery.ServiceMaps
{
    public class ServiceNodeConfig
    {
        public ServiceNodeConfig(
            ServiceEndPoint serviceEndPoint,
            IEnumerable<ServiceEndPoint> services,
            bool isReachable)
        {
            ServiceEndPoint = serviceEndPoint;
            Services = services;
            IsReachable = isReachable;
        }

        public bool IsReachable { get; }
        public ServiceEndPoint ServiceEndPoint { get; }
        public IEnumerable<ServiceEndPoint> Services { get; }
    }
}