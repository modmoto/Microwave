using System.Threading.Tasks;
using Microwave.Discovery;
using Microwave.Discovery.EventLocations;
using Microwave.Discovery.ServiceMaps;

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
            MicrowaveServiceNode microwaveServiceNode,
            EventSchema subscribedEvent)
        {
//            var serviceBaseAddress = microwaveServiceNode.ServiceEndPoint.ServiceBaseAddress;
//            var httpClient = await _httpClientFactory.CreateHttpClient(serviceBaseAddress);
            var appBaseUrl = _microwaveHttpContext.AppBaseUrl;
        }
    }
}