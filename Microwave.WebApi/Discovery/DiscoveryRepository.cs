using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using Newtonsoft.Json;

namespace Microwave.WebApi.Discovery
{
    public class DiscoveryRepository : IServiceDiscoveryRepository
    {
        private readonly IDiscoveryClientFactory _factory;

        public DiscoveryRepository(IDiscoveryClientFactory factory)
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

                return EventsPublishedByService.Reachable(new ServiceEndPoint(serviceAdress, events.ServiceName)
                    , events.PublishedEvents);
            }
            catch (HttpRequestException)
            {
                return EventsPublishedByService.NotReachable(new ServiceEndPoint(serviceAdress));
            }
        }

        public async Task<MicrowaveServiceNode> GetDependantServices(Uri serviceAddress)
        {
            var client = _factory.GetClient(serviceAddress);
            try
            {
                var response = await client.GetAsync("Dicovery/ServiceDependencies");
                if (!response.IsSuccessStatusCode) return MicrowaveServiceNode.UnreachableMicrowaveServiceNode(new ServiceEndPoint(serviceAddress), new List<ServiceEndPoint>());
                var content = await response.Content.ReadAsStringAsync();
                var serviceDependencies = JsonConvert.DeserializeObject<MicrowaveServiceNode>(content);
                serviceDependencies.SetAddressForEndPoint(serviceAddress);
                return serviceDependencies;
            }
            catch (HttpRequestException)
            {
                return MicrowaveServiceNode.UnreachableMicrowaveServiceNode(new ServiceEndPoint(serviceAddress), new List<ServiceEndPoint>());
            }
        }
    }
}