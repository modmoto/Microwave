using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microwave.WebApi.Discovery
{
    internal class DiscoveryClientFactory : IDiscoveryClientFactory
    {
        private readonly IMicrowaveHttpClientFactory _clientFactory;

        public DiscoveryClientFactory(IMicrowaveHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<HttpClient> GetClient(Uri serviceAddress)
        {
            var discoveryClient = await _clientFactory.CreateHttpClient(serviceAddress);
            discoveryClient.BaseAddress = serviceAddress;
            return discoveryClient;
        }
    }

    public interface IDiscoveryClientFactory
    {
        Task<HttpClient> GetClient(Uri serviceAddress);
    }
}