using System.Net.Http;
using System.Threading.Tasks;
using Microwave.Discovery;
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
            Subscription subscription)
        {
            var serviceToSubscribeTo = subscription.SubscriberUrl;
            var subscriberUrl = _microwaveHttpContext.AppBaseUrl;
            var httpClient = await _httpClientFactory.CreateHttpClient(serviceToSubscribeTo);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "Discovery/Subscriptions");
            string serialized = JsonConvert.SerializeObject(new { subscription.SubscribedEvent, subscriberUrl });
            httpRequestMessage.Content = new StringContent(serialized);
            await httpClient.SendAsync(httpRequestMessage);
        }
    }
}