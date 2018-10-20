using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Framework;

namespace Adapters.Framework.EventStores
{
    public class MyEventStore : IEventStoreFacade
    {
        private readonly IEventRepository _eventRepository;

        public MyEventStore(IEventRepository eventRepository )
        {
            _eventRepository = eventRepository;
        }
        public async Task AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion)
        {
            await _eventRepository.AppendAsync(domainEvents, entityVersion);
        }

        public async Task<T> LoadAsync<T>(Guid entityId) where T : Entity, new()
        {
            var entity = new T();
            var domainEvents = await _eventRepository.LoadEvents(entityId);
            return domainEvents.Aggregate(entity, (current, domainEvent) => Apply(current, domainEvent));
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