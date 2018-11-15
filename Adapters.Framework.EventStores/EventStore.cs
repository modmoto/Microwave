using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Framework;

namespace Adapters.Framework.EventStores
{
    public class EventStore : IEventStore
    {
        private readonly IEntityStreamRepository _entityStreamRepository;

        public EventStore(IEntityStreamRepository entityStreamRepository )
        {
            _entityStreamRepository = entityStreamRepository;
        }

        public async Task AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion)
        {
            var result = await _entityStreamRepository.AppendAsync(domainEvents, entityVersion);
            result.Check();
        }

        public async Task<EventstoreResult<T>> LoadAsync<T>(Guid entityId) where T : new()
        {
            var entity = new T();
            var domainEvents = (await _entityStreamRepository.LoadEventsByEntity(entityId)).Value;
            var eventList = domainEvents.ToList();
            entity = eventList.Aggregate(entity, (current, domainEvent) => Apply(current, domainEvent));
            // TODO get this from stream/events
            return new EventstoreResult<T>(eventList.Count - 1, entity);
        }

        private T Apply<T>(T entity, DomainEvent domainEvent)
        {
            var type = domainEvent.GetType();
            var currentEntityType = entity.GetType();
            var methodInfos = currentEntityType.GetMethods().Where(method => method.Name == "Apply");
            var methodToExecute = methodInfos.FirstOrDefault(method => method.GetParameters().FirstOrDefault()?.ParameterType == type);
            if (methodToExecute == null || methodToExecute.GetParameters().Length != 1) return entity;
            methodToExecute.Invoke(entity, new object[] {domainEvent});
            return entity;
        }
    }
}