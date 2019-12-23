using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Domain.Results;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Microwave.Queries.Handler;
using Microwave.Queries.Ports;

namespace Microwave
{
    public class LocalEventFeed<T> : IEventFeed<T>
    {
        private readonly IEventRepository _eventRepository;

        public LocalEventFeed(IEventRepository eventRepository)
        {
            _eventRepository = eventRepository;
        }

        public async Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(long lastVersion = 0)
        {
            var loadEvents = await LoadEventsAccordingToT(lastVersion);
            var wrappers = loadEvents.Value.Select(e => new SubscribedDomainEventWrapper
            {
                EntityStreamVersion = e.EntityStreamVersion,
                OverallVersion = e.OverallVersion,
                DomainEvent = (dynamic) e.DomainEvent
            });

            return wrappers;
        }

        private Task<Result<IEnumerable<DomainEventWrapper>>> LoadEventsAccordingToT(long lastVersion)
        {
            var type = typeof(T);
            if (typeof(IAsyncEventHandler).IsAssignableFrom(type))
            {
                var eventType = type.GetGenericArguments().First().Name;
                return _eventRepository.LoadEventsByTypeAsync(eventType, lastVersion);
            }

            if (typeof(IQueryEventHandler).IsAssignableFrom(type))
            {
                var eventType = type.GetGenericArguments().Skip(1).First().Name;
                return _eventRepository.LoadEventsByTypeAsync(eventType, lastVersion);
            }

            return _eventRepository.LoadEvents(lastVersion);
        }
    }
}