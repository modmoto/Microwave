using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Microwave.WebApi.Discovery
{
    public class DiscoveryClientFactory : IDiscoveryClientFactory
    {
        private readonly IMicrowaveHttpClientCreator _clientCreator;

        public DiscoveryClientFactory(IMicrowaveHttpClientCreator clientCreator)
        {
            _clientCreator = clientCreator;
        }

        public async Task<HttpClient> GetClient(Uri serviceAddress)
        {
            var discoveryClient = await _clientCreator.CreateHttpClient(serviceAddress);
            discoveryClient.BaseAddress = serviceAddress;
            return discoveryClient;
        }
    }

    public interface IDiscoveryClientFactory
    {
        Task<HttpClient> GetClient(Uri serviceAddress);
    }
}