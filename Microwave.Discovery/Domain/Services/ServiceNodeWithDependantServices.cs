using System;
using System.Collections.Generic;

namespace Microwave.Discovery.Domain.Services
{
    public class ServiceNodeWithDependantServices
    {
        public ServiceNodeWithDependantServices(
            string serviceName,
            IEnumerable<ServiceEndPoint> services)
        {
            ServiceName = serviceName;
            Services = services;
            IsReachable = true;
        }

        private ServiceNodeWithDependantServices(string serviceName,
            IEnumerable<ServiceEndPoint> services, bool isReachable)
        {
            ServiceName = serviceName;
            Services = services;
            IsReachable = isReachable;
        }

        public string ServiceName { get; }
        public bool IsReachable { get; }
        public IEnumerable<ServiceEndPoint> Services { get; }

        public static ServiceNodeWithDependantServices NotReachable(Uri serviceAdress)
        {
            return new ServiceNodeWithDependantServices(serviceAdress.ToString(), new List<ServiceEndPoint>(), false);
        }
    }
}