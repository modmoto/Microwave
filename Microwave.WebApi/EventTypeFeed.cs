using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.Application.Ports;
using Microwave.Domain;
using Microwave.ObjectPersistences;

namespace Microwave.WebApi
{
    public class EventTypeFeed<T> : IEventFeed<T> where T : IDomainEvent
    {
        private readonly DomainEventWrapperListDeserializer _objectConverter;
        private readonly DomainEventClient<T> _domainEventClient;

        public EventTypeFeed(DomainEventWrapperListDeserializer objectConverter, DomainEventClient<T>
            domainEventClient)
        {
            _objectConverter = objectConverter;
            _domainEventClient = domainEventClient;
        }

        public async Task<IEnumerable<DomainEventHto<T>>> GetEventsAsync(long lastVersion)
        {
            var response = await _domainEventClient.GetAsync($"?timeStamp={lastVersion}");
            if (response.StatusCode != HttpStatusCode.OK) return new List<DomainEventHto<T>>();
            var content = await response.Content.ReadAsStringAsync();
            var eventsByTypeAsync = _objectConverter.Deserialize<T>(content);
            return eventsByTypeAsync;
        }
    }
}