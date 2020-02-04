using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Results;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Microwave.Queries;
using Microwave.Queries.Handler;
using Microwave.Queries.Ports;
using Newtonsoft.Json;

namespace Microwave
{
    public class LocalEventFeed<T> : IEventFeed<T>
    {
        private readonly IEventRepository _eventRepository;
        private Type _eventType;

        public LocalEventFeed(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(long lastVersion = 0)
        {
            var loadEvents = await LoadEventsAccordingToT(lastVersion);
            var wrappers = loadEvents.Value.Select(e =>
            {
                var subscribedDomainEvent = ParseToSubscribedEvent(e.DomainEvent);
                if (subscribedDomainEvent == null) return null;
                return new SubscribedDomainEventWrapper
                {
                    EntityStreamVersion = e.EntityStreamVersion,
                    OverallVersion = e.OverallVersion,
                    DomainEvent = subscribedDomainEvent
                };
            }).Where(e => e != null);

            return wrappers;
        }

        private ISubscribedDomainEvent ParseToSubscribedEvent(IDomainEvent domainEvent)
        {
            var readModelType = typeof(T).GetGenericArguments().First();
            if (typeof(ReadModelBase).IsAssignableFrom(readModelType))
            {
                var interfaces = readModelType.GetInterfaces();
                var applicableEvents = interfaces
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IHandle<>))
                    .Select(h => h.GetGenericArguments().First());
                var name = domainEvent.GetType().Name;
                var subscribedEventType = applicableEvents.FirstOrDefault(e => e.Name == name);
                if (subscribedEventType == null) return null;
                _eventType = subscribedEventType;
            }

            var serializeObject = JsonConvert.SerializeObject(domainEvent);
            var deserializeObject = JsonConvert.DeserializeObject(serializeObject, _eventType);
            return deserializeObject as ISubscribedDomainEvent;
        }

        private Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsAccordingToT(long lastVersion)
        {
            var type = typeof(T);
            if (type.GetGenericTypeDefinition() == typeof(AsyncEventHandler<,>))
            {
                _eventType = type.GetGenericArguments().First();
                return _eventRepository.LoadEventsByTypeAsync(_eventType.Name, lastVersion);
            }

            if (type.GetGenericTypeDefinition() == typeof(QueryEventHandler<,>))
            {
                _eventType = type.GetGenericArguments().Skip(1).First();
                return _eventRepository.LoadEventsByTypeAsync(_eventType.Name, lastVersion);
            }

            return _eventRepository.LoadEvents(lastVersion);
        }
    }
}