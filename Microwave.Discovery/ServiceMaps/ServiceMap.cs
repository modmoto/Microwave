using System.Collections.Generic;

namespace Microwave.Discovery.ServiceMaps
{
    public class ServiceMap
    {
        public IEnumerable<MicrowaveServiceNode> AllServices { get; }

        public ServiceMap(IEnumerable<MicrowaveServiceNode> allServices)
        {
            AllServices = allServices;
        }
    }
}