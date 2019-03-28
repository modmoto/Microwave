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
        private HttpClient _client;

        public ServiceDiscoveryRepository(DiscoveryClient client)
        {
            _client = client;
        }
        public async Task<PublisherEventConfig> GetPublishedEventTypes(Uri serviceAdress)
        {
            _client.BaseAddress = serviceAdress;

            try
            {
                var response = await _client.GetAsync("Dicovery/PublishedEvents");
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