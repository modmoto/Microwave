using System;
using Microwave.Application.Discovery;

namespace ServerConfig
{
    public class ServiceConfiguration
    {
        public static ServiceBaseAddressCollection ServiceAdresses => new ServiceBaseAddressCollection
        {
            new Uri("http://localhost:6001"),
            new Uri("http://localhost:6002"),
            new Uri("http://localhost:6003")
        };
    }
}