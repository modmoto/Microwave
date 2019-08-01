using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microwave.Queries.Ports;

namespace Microwave.WebApi.Queries
{
    internal class EventFeed<T> : IEventFeed<T>
    {
        private readonly IDomainEventFactory _eventFactory;
        private readonly IDomainEventClientFactory _clientFactory;

        public EventFeed(
            IDomainEventFactory eventFactory,
            IDomainEventClientFactory clientFactory)
        {
            _eventFactory = eventFactory;
            _clientFactory = clientFactory;
        }

        public async Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset))
        {
            if (since == default(DateTimeOffset)) since = DateTimeOffset.MinValue;
            var isoString = since.ToString("o");
            var client = await _clientFactory.GetClient<T>();
            try
            {
                if (client.BaseAddress != null) {
                    var response = await client.GetAsync($"?timeStamp={isoString}");
                    if (!response.IsSuccessStatusCode) return new List<SubscribedDomainEventWrapper>();
                    var content = await response.Content.ReadAsStringAsync();
                    var eventsByTypeAsync = _eventFactory.Deserialize(content);
                    return eventsByTypeAsync;
                }
            }
            catch (HttpRequestException)
            {
                var type = typeof(T);
                var readModel = type.GenericTypeArguments.Single();
                Console.WriteLine($"Could not reach service for: {readModel.Name}");
            }

            return new List<SubscribedDomainEventWrapper>();
        }
    }
}