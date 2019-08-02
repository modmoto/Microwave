using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microwave.Discovery.ServiceMaps;

[assembly: InternalsVisibleTo("Microwave.WebApi.UnitTests")]
namespace Microwave.WebApi.Discovery
{
    public class MicrowaveServiceNodeDto
    {
        public ServiceEndPoint ServiceEndPoint { get; set; }
        public IEnumerable<ServiceEndPoint> ConnectedServices { get; set; }
    }
}