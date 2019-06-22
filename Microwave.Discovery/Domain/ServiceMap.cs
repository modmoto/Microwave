using System.Collections.Generic;
using Microwave.Discovery.Domain.Services;

namespace Microwave.Discovery.Domain
{
    public class ServiceMap
    {
        public IEnumerable<ServiceNodeConfig> AllServices { get; }

        public ServiceMap(IEnumerable<ServiceNodeConfig> allServices)
        {
            AllServices = allServices;
        }
    }
}