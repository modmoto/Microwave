using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Application.Framework;
using Microwave.Domain;

namespace Adapters.Framework.WebApi
{
    public class EventFeed<T> : IEventFeed<T> where T : IDomainEvent
    {
        private readonly IObjectConverter _objectConverter;
        private readonly DomainEventClient<T> _domainEventClient;

        public EventFeed(IObjectConverter objectConverter, DomainEventClient<T>
            domainEventClient)
        {
            _objectConverter = objectConverter;
            _domainEventClient = domainEventClient;
        }

        public async Task<IEnumerable<T>> GetEventsByTypeAsync(long lastVersion)
        {
            var response = await _domainEventClient.GetAsync($"?myLastVersion={lastVersion}");
            if (response.StatusCode != HttpStatusCode.OK) return new List<T>();
            var content = await response.Content.ReadAsStringAsync();
            var eventsByTypeAsync = _objectConverter.Deserialize<IEnumerable<DomainvEventWrapper<T>>>(content);
            return eventsByTypeAsync.Select(ev => ev.DomainEvent);
        }
    }

    public class DomainvEventWrapper<TEvent> where TEvent : IDomainEvent
    {
        public DomainvEventWrapper(TEvent domainEvent)
        {
            DomainEvent = domainEvent;
        }

        public TEvent DomainEvent { get; }
    }
}