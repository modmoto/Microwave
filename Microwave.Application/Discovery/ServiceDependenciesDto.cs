using System;
using System.Collections.Generic;

namespace Microwave.Application.Discovery
{
    public class ServiceDependenciesDto
    {
        public string ServiceName { get; set; }
        public Uri ServiceBaseAddress { get; set; }
        public bool IsReachable { get; set; }
        public IEnumerable<ServiceDependenciesDto> DependantServices { get; set; }
    }
}