using System;
using System.Collections.Generic;

namespace Microwave.Discovery.Domain.Services
{
    public class ServiceNodeWithDependentServicesDto
    {
        public ServiceNodeWithDependentServicesDto(
            string serviceName,
            IEnumerable<ServiceEndPoint> services)
        {
            ServiceName = serviceName;
            Services = services;
            IsReachable = true;
        }

        private ServiceNodeWithDependentServicesDto(string serviceName,
            IEnumerable<ServiceEndPoint> services, bool isReachable)
        {
            ServiceName = serviceName;
            Services = services;
            IsReachable = isReachable;
        }

        public string ServiceName { get; }
        public bool IsReachable { get; }
        public IEnumerable<ServiceEndPoint> Services { get; }

        public static ServiceNodeWithDependentServicesDto NotReachable(Uri serviceAdress)
        {
            return new ServiceNodeWithDependentServicesDto(serviceAdress.ToString(), new List<ServiceEndPoint>(), false);
        }
    }
}