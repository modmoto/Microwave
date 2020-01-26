using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microwave.Logging;
using Microwave.Queries.Ports;

namespace Microwave.WebApi.Queries
{
    public class EventFeed<T> : IEventFeed<T>
    {
        private readonly IDomainEventFactory _eventFactory;
        private readonly IDomainEventClientFactory _clientFactory;
        private readonly IMicrowaveLogger<EventFeed<T>> _logger;

        public EventFeed(
            IDomainEventFactory eventFactory,
            IDomainEventClientFactory clientFactory,
            IMicrowaveLogger<EventFeed<T>> logger)
        {
            _eventFactory = eventFactory;
            _clientFactory = clientFactory;
            _logger = logger;
        }

        public async Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(long lastVersion = 0)
        {
            var client = await _clientFactory.GetClient<T>();
            try
            {
                if (client.BaseAddress != null) {
                    var response = await client.GetAsync($"?lastVersion={lastVersion}");
                    Console.WriteLine($"Response for Event Call was {response.StatusCode}");
                    if (!response.IsSuccessStatusCode) return new List<SubscribedDomainEventWrapper>();
                    var content = await response.Content.ReadAsStringAsync();
                    var eventsByTypeAsync = _eventFactory.Deserialize(content).ToList();
                    Console.WriteLine($"Retrieved {eventsByTypeAsync.Count} events");
                    return eventsByTypeAsync;
                }
                Console.WriteLine($"Base Adress was null, call avoided");
            }
            catch (HttpRequestException e)
            {
                var type = typeof(T);
                var readModel = type.GenericTypeArguments.Single();
                _logger.LogWarning(e, $"Could not reach service for: {readModel.Name}");
            }

            return new List<SubscribedDomainEventWrapper>();
        }
    }
}