using System.Net.Http;
using System.Threading.Tasks;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;
using Newtonsoft.Json;

namespace Microwave.WebApi.Discovery
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly IMicrowaveHttpClientFactory _httpClientFactory;
        private readonly MicrowaveHttpContext _microwaveHttpContext;

        public SubscriptionRepository(
            IMicrowaveHttpClientFactory httpClientFactory,
            MicrowaveHttpContext microwaveHttpContext)
        {
            _httpClientFactory = httpClientFactory;
            _microwaveHttpContext = microwaveHttpContext;
        }

        public async Task SubscribeForEvent(
            ServiceEndPoint microwaveServiceNode,
            EventSchema subscribedEvent)
        {
            var serviceToSubscribeTo = microwaveServiceNode.ServiceBaseAddress;
            var subscriberUrl = _microwaveHttpContext.AppBaseUrl;
            var httpClient = await _httpClientFactory.CreateHttpClient(serviceToSubscribeTo);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "Discovery/Subscriptions");
            string serialized = JsonConvert.SerializeObject(new { subscribedEvent, subscriberUrl });
            httpRequestMessage.Content = new StringContent(serialized);
            await httpClient.SendAsync(httpRequestMessage);
        }
    }
}