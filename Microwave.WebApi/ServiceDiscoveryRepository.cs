using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.Application.Discovery;
using Newtonsoft.Json;

namespace Microwave.WebApi
{
    public class ServiceDiscoveryRepository : IServiceDiscoveryRepository
    {
        public async Task<ConsumingService> GetPublishedEventTypes(Uri serviceAdress)
        {
            var client = new HttpClient();
            client.BaseAddress = serviceAdress;

            var response = await client.GetAsync("Dicovery/PublishedEvents");
            if (response.StatusCode != HttpStatusCode.OK) return new ConsumingService(
                serviceAdress,
                new List<string>(),
                "Service unavailable");

            var content = await response.Content.ReadAsStringAsync();
            var eventsByTypeAsync = JsonConvert.DeserializeObject<PublishedEventCollection>(content);

            return new ConsumingService(serviceAdress, eventsByTypeAsync);
        }
    }
}