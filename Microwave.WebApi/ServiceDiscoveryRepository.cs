using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microwave.Application.Discovery;
using Newtonsoft.Json;

namespace Microwave.WebApi
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

                return new EventsPublishedByService(serviceAdress, events.PublishedEvents, true, events.ServiceName);
            }
            catch (HttpRequestException)
            {
                return new EventsPublishedByService(serviceAdress, new List<EventSchema>(), false);
            }
        }

        public async Task<ServiceDependenciesDto> GetDependantServices(Uri serviceAdress)
        {
            var client = _factory.GetClient(serviceAdress);
            try
            {
                var response = await client.GetAsync("Dicovery/ServiceDependencies");
                var content = await response.Content.ReadAsStringAsync();
                var serviceDependencies = JsonConvert.DeserializeObject<ServiceDependenciesDto>(content);
                serviceDependencies.IsReachable = true;
                return serviceDependencies;
            }
            catch (HttpRequestException)
            {
                return new ServiceDependenciesDto
                {
                    IsReachable = false
                };
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