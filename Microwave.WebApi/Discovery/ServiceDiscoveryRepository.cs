using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microwave.Discovery;
using Newtonsoft.Json;

namespace Microwave.WebApi.Discovery
{
    public class ServiceDiscoveryRepository : IServiceDiscoveryRepository
    {
        private readonly IDiscoveryClientFactory _factory;

        public ServiceDiscoveryRepository(IDiscoveryClientFactory factory)
        {
            _factory = factory;
        }
        public async Task<EventsPublishedByService> GetPublishedEventTypes(Uri serviceAdress)
        {
            var client = _factory.GetClient(serviceAdress);
            try
            {
                var response = await client.GetAsync("Dicovery/PublishedEvents");
                var content = await response.Content.ReadAsStringAsync();
                var events = JsonConvert.DeserializeObject<PublishedEventsByServiceDto>(content);

                return new EventsPublishedByService(new NodeEntryPoint(serviceAdress, events.ServiceName)
                    , events.PublishedEvents);
            }
            catch (HttpRequestException)
            {
                return new EventsPublishedByService(new NodeEntryPoint(serviceAdress), new List<EventSchema>(), false);
            }
        }

        public async Task<ServiceNode> GetDependantServices(Uri serviceAdress)
        {
            var client = _factory.GetClient(serviceAdress);
            try
            {
                var response = await client.GetAsync("Dicovery/ServiceDependencies");
                var content = await response.Content.ReadAsStringAsync();
                var serviceDependencies = JsonConvert.DeserializeObject<EventLocationDto>(content);
                return ServiceNode.Ok(new NodeEntryPoint(serviceAdress, serviceDependencies.ServiceName), serviceDependencies.Services);
            }
            catch (HttpRequestException)
            {
                return ServiceNode.NotReachable(new NodeEntryPoint(serviceAdress));
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