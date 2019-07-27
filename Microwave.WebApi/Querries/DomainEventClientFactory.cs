using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microwave.Discovery;
using Microwave.Queries.Handler;

namespace Microwave.WebApi.Querries
{
    public interface IDomainEventClientFactory
    {
        Task<HttpClient> GetClient<T>();
    }

    public class DomainEventClientFactory : IDomainEventClientFactory
    {
        private readonly IStatusRepository _statusRepository;
        private readonly IMicrowaveHttpClientFactory _httpClientFactory;

        public DomainEventClientFactory(IStatusRepository statusRepository, IMicrowaveHttpClientFactory httpClientFactory)
        {
            _statusRepository = statusRepository;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<HttpClient> GetClient<T>()
        {
            var eventLocation = await _statusRepository.GetEventLocation();
            var type = typeof(T);
            if (typeof(IAsyncEventHandler).IsAssignableFrom(type))
            {
                var eventType = type.GetGenericArguments().First();
                var domainEventLocation = eventLocation.GetServiceForEvent(eventType);
                if (domainEventLocation == null) return DefaultHttpClient();
                var httpClient = await _httpClientFactory.CreateHttpClient(domainEventLocation.ServiceEndPoint
                .ServiceBaseAddress);
                httpClient.BaseAddress = new Uri(domainEventLocation.ServiceEndPoint.ServiceBaseAddress + $"Api/DomainEventTypeStreams/{eventType.Name}");
                return httpClient;
            }
            else if (typeof(IQueryEventHandler).IsAssignableFrom(type))
            {
                var eventType = type.GetGenericArguments().Skip(1).First();
                var consumingService = eventLocation.GetServiceForEvent(eventType);
                if (consumingService == null) return DefaultHttpClient();
                var httpClient = await _httpClientFactory.CreateHttpClient(consumingService.ServiceEndPoint
                .ServiceBaseAddress);
                httpClient.BaseAddress = new Uri(consumingService.ServiceEndPoint.ServiceBaseAddress + $"Api/DomainEventTypeStreams/{eventType.Name}");
                return httpClient;
            }
            else
            {
                var readModelType = type.GetGenericArguments().First();
                var subscriberEventAndReadmodelConfig = eventLocation.GetServiceForReadModel(readModelType);
                if (subscriberEventAndReadmodelConfig == null) return DefaultHttpClient();
                var httpClient = await _httpClientFactory.CreateHttpClient(subscriberEventAndReadmodelConfig
                .ServiceEndPoint.ServiceBaseAddress);
                httpClient.BaseAddress = new Uri(subscriberEventAndReadmodelConfig.ServiceEndPoint.ServiceBaseAddress + "Api/DomainEvents");
                return httpClient;
            }
        }

        private static HttpClient DefaultHttpClient() => new HttpClient { BaseAddress = null };
    }
}