using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.Domain;
using Microwave.ObjectPersistences;

namespace Microwave.WebApi
{
    public class EventFeed<T> : IEventFeed<T> where T : IDomainEvent
    {
        private readonly DomainEventWrapperListDeserializer _objectConverter;
        private readonly DomainEventClient<T> _domainEventClient;

        public EventFeed(DomainEventWrapperListDeserializer objectConverter, DomainEventClient<T>
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
            var eventsByTypeAsync = _objectConverter.Deserialize(content);
            return eventsByTypeAsync.Select(ev => (T) ev.DomainEvent);
        }
    }
}