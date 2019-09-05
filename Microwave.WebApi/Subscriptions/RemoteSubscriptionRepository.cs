using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microwave.Subscriptions;
using Microwave.Subscriptions.Ports;
using Microwave.WebApi.Discovery;
using Newtonsoft.Json;

namespace Microwave.WebApi.Subscriptions
{
    public class RemoteSubscriptionRepository : IRemoteSubscriptionRepository
    {
        private readonly IMicrowaveHttpClientFactory _httpClientFactory;
        private readonly MicrowaveHttpContext _microwaveHttpContext;

        public RemoteSubscriptionRepository(
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
            httpClient.BaseAddress = serviceToSubscribeTo;
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "Subscriptions/Subscriptions");
            var serialized = JsonConvert.SerializeObject(new { subscription.SubscribedEvent, subscriberUrl });
            httpRequestMessage.Content = new StringContent(serialized, Encoding.UTF8, "application/json");
            await httpClient.SendAsync(httpRequestMessage);
        }

        public async Task PushChangesForType(Uri remoteService, string eventType, DateTimeOffset newVersion)
        {
            var httpClient = await _httpClientFactory.CreateHttpClient(remoteService);
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"Subscriptions/Subscriptions/{eventType}");
            string serialized = JsonConvert.SerializeObject(new { newVersion });
            httpRequestMessage.Content = new StringContent(serialized, Encoding.UTF8, "application/json");
            httpClient.BaseAddress = remoteService;
            await httpClient.SendAsync(httpRequestMessage);
        }

        public async Task PushChangesForAll(Uri remoteService, DateTimeOffset newVersion)
        {
            var httpClient = await _httpClientFactory.CreateHttpClient(remoteService);
            httpClient.BaseAddress = remoteService;
            var httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, "Subscriptions/Subscriptions");
            string serialized = JsonConvert.SerializeObject(new { newVersion });
            httpRequestMessage.Content = new StringContent(serialized, Encoding.UTF8, "application/json");
            await httpClient.SendAsync(httpRequestMessage);
        }
    }
}