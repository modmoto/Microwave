using System.Collections.Generic;

namespace Microwave.Discovery.ServiceMaps
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