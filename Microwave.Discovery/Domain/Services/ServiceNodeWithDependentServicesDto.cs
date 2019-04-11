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
        }

        public string ServiceName { get; }
        public IEnumerable<ServiceEndPoint> Services { get; }
    }
}