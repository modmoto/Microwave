using System;
using Microwave;
using Microwave.Domain;

namespace ServerConfig
{
    public class ServiceConfiguration
    {
        public static IServiceBaseAddressCollection ServiceAdresses => new ServiceBaseAddressCollection
        {
            new Uri("http://localhost:5010"),
            new Uri("http://localhost:5012"),
            new Uri("http://localhost:5014"),
            new Uri("http://localhost:5016"), //undiscovered service
        };
    }
}