using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microwave.Application;
using Microwave.Domain;

namespace Microwave.EventStores
{
    public class EventStore : IEventStore
    {
        private readonly IEntityStreamRepository _entityStreamRepository;

        public EventStore(IEntityStreamRepository entityStreamRepository)
        {
            _entityStreamRepository = entityStreamRepository;
        }

        public async Task AppendAsync(IEnumerable<IDomainEvent> domainEvents, long entityVersion)
        {
            var result = await _entityStreamRepository.AppendAsync(domainEvents, entityVersion);
            result.Check();
        }

        public async Task<EventstoreResult<T>> LoadAsync<T>(Guid entityId) where T : new()
        {
            var entity = new T();
            var domainEvents = (await _entityStreamRepository.LoadEventsByEntity(entityId)).Value;
            var eventList = domainEvents.ToList();
            entity = eventList.Aggregate(entity, (current, domainEvent) => Apply(current, domainEvent.DomainEvent));
            // TODO get this from stream/events
            return new EventstoreResult<T>(eventList.Count, entity);
        }

        private T Apply<T>(T entity, IDomainEvent domainEvent)
        {
            var interfaces = entity.GetType().GetInterfaces();
            var applyInterfaces = interfaces.Where(inte => inte.GetGenericTypeDefinition() == typeof(IApply<>));
            var applyInterfaceForType = applyInterfaces.FirstOrDefault(af => af.GetGenericArguments().Single() == domainEvent.GetType());
            var applyMethod = applyInterfaceForType?.GetMethod(nameof(IApply<IDomainEvent>.Apply));
            applyMethod?.Invoke(entity, new object[] {domainEvent});
            return entity;
        }
    }
}