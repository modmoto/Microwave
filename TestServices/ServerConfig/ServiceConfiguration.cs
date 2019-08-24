using System;
using Microwave;

namespace ServerConfig
{
    public class ServiceConfiguration
    {
        public static ServiceBaseAddressCollection ServiceAdresses => new ServiceBaseAddressCollection
       {
            new Uri("http://localhost:5010"),
            new Uri("http://localhost:5012"),
            new Uri("http://localhost:5014"),
        };
    }
}