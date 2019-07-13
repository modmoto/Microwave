using System;
using System.Net.Http;

namespace Microwave.WebApi.Discovery
{
    public class DiscoveryClientFactory : IDiscoveryClientFactory
    {
        private readonly IMicrowaveHttpClientCreator _clientCreator;

        public DiscoveryClientFactory(IMicrowaveHttpClientCreator clientCreator)
        {
            _clientCreator = clientCreator;
        }

        public HttpClient GetClient(Uri serviceAddress)
        {
            var discoveryClient = _clientCreator.CreateHttpClient(serviceAddress);
            discoveryClient.BaseAddress = serviceAddress;
            return discoveryClient;
        }
    }

    public interface IDiscoveryClientFactory
    {
        HttpClient GetClient(Uri serviceAddress);
    }

    public class  DiscoveryClient : HttpClient
    {
        // For DI
        public DiscoveryClient()
        {
        }

        public DiscoveryClient(HttpMessageHandler handler) : base(handler)
        {
        }
    }
}