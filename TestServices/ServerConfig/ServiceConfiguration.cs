using System;
using Microwave.Discovery;

namespace ServerConfig
{
    public class ServiceConfiguration
    {
        public static ServiceBaseAddressCollection ServiceAdresses => new ServiceBaseAddressCollection
        {
            new Uri("http://localhost:5000"),
            new Uri("http://localhost:5001"),
            new Uri("http://localhost:5002"),
        };
    }
}