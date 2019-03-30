using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.Application.Discovery;
using Newtonsoft.Json;

namespace Microwave.WebApi
{
    public class ServiceDiscoveryRepository : IServiceDiscoveryRepository
    {
        private IDiscoveryClientFactory _client;

        public ServiceDiscoveryRepository(IDiscoveryClientFactory client)
        {
            _client = client;
        }
        public async Task<PublisherEventConfig> GetPublishedEventTypes(Uri serviceAdress)
        {
            var client = _client.GetClient(serviceAdress);
            try
            {
                var response = await client.GetAsync("Dicovery/PublishedEvents");
                var content = await response.Content.ReadAsStringAsync();
                var eventsByTypeAsync = JsonConvert.DeserializeObject<PublishedEventCollection>(content);

                return new PublisherEventConfig(serviceAdress, eventsByTypeAsync);
            }
            catch (HttpRequestException)
            {
                return new PublisherEventConfig(serviceAdress, new List<string>(), false);
            }
        }
    }

    public class DiscoveryClientFactory : IDiscoveryClientFactory
    {
        public DiscoveryClient GetClient(Uri serviceAdress)
        {
            var discoveryClient = new DiscoveryClient();
            discoveryClient.BaseAddress = serviceAdress;
            return discoveryClient;
        }
    }

    public interface IDiscoveryClientFactory
    {
        DiscoveryClient GetClient(Uri serviceAdress);
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