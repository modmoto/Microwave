using System;

namespace Microwave.Discovery.Domain.Services
{
    public class ServiceEndPoint
    {
        public ServiceEndPoint(Uri serviceBaseAddress, string name = null)
        {
            Name = name ?? serviceBaseAddress.ToString();
            ServiceBaseAddress = serviceBaseAddress;
        }

        public string Name { get; }
        public Uri ServiceBaseAddress { get; }
    }
}