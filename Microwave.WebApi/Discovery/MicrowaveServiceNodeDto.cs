using System.Collections.Generic;
using Microwave.Discovery.ServiceMaps;

namespace Microwave.WebApi.Discovery
{
    public class MicrowaveServiceNodeDto
    {
        public ServiceEndPoint ServiceEndPoint { get; set; }
        public IEnumerable<ServiceEndPoint> ConnectedServices { get; set; }
    }
}