using System;
using System.Net.Http;
using Microwave.Domain;

namespace Microwave.WebApi.Discovery
{
    public class DiscoveryClientFactory : IDiscoveryClientFactory
    {
        private readonly IMicrowaveHttpClientCreator _clientCreator;

        public DiscoveryClientFactory(IMicrowaveHttpClientCreator clientCreator)
        {
            _clientCreator = clientCreator;
        }

        public HttpClient GetClient(Uri serviceAdress)
        {
            var discoveryClient = _clientCreator.CreateHttpClient();
            discoveryClient.BaseAddress = serviceAdress;
            return discoveryClient;
        }
    }

    public interface IDiscoveryClientFactory
    {
        HttpClient GetClient(Uri serviceAdress);
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