using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using Newtonsoft.Json;


[assembly: InternalsVisibleTo("Microwave")]
[assembly: InternalsVisibleTo("Microwave.Discovery.UnitTests")]
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
            var client = await _factory.GetClient(serviceAdress);
            try
            {
                var response = await client.GetAsync("Dicovery/PublishedEvents");
                if (!response.IsSuccessStatusCode) return EventsPublishedByService.NotReachable(new ServiceEndPoint(serviceAdress));
                var content = await response.Content.ReadAsStringAsync();
                var events = JsonConvert.DeserializeObject<PublishedEventsByServiceDto>(content);

                return EventsPublishedByService.Reachable(
                    new ServiceEndPoint(serviceAdress, events.ServiceName),
                    events.PublishedEvents);
            }
            catch (HttpRequestException)
            {
                return EventsPublishedByService.NotReachable(new ServiceEndPoint(serviceAdress));
            }
            catch (IOException)
            {
                return EventsPublishedByService.NotReachable(new ServiceEndPoint(serviceAdress));
            }
        }

        public async Task<MicrowaveServiceNode> GetDependantServices(Uri serviceAddress)
        {
            var client = await _factory.GetClient(serviceAddress);
            try
            {
                var response = await client.GetAsync("Dicovery/ServiceDependencies");
                if (!response.IsSuccessStatusCode) return MicrowaveServiceNode.UnreachableMicrowaveServiceNode(new ServiceEndPoint(serviceAddress), new List<ServiceEndPoint>());
                var content = await response.Content.ReadAsStringAsync();
                var remoteNode = JsonConvert.DeserializeObject<MicrowaveServiceNodeDto>(content);
                var newNode = MicrowaveServiceNode.ReachableMicrowaveServiceNode(
                    new ServiceEndPoint(serviceAddress, remoteNode.ServiceEndPoint.Name),
                    remoteNode.ConnectedServices);
                return newNode;
            }
            catch (HttpRequestException)
            {
                return MicrowaveServiceNode.UnreachableMicrowaveServiceNode(new ServiceEndPoint(serviceAddress), new List<ServiceEndPoint>());
            }
        }
    }
}