using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Framework.Results;
using Domain.Framework;

namespace Application.Framework
{
    public interface IEventStoreFacade
    {
        Task AppendAsync(IEnumerable<DomainEvent> domainEvents, long entityVersion);
        Task<EventStoreResult<T>> LoadAsync<T>(Guid commandEntityId, bool loadNestedEntities = false) where T : new();
        Task<EventStoreResult<IEnumerable<DomainEvent>>>  GetEvents(Guid entityId = default(Guid), int from = 0, int to = 100);
        Task AppendAsync(IEnumerable<DomainEvent> domainResultDomainEvents);
        Task Subscribe(Type domainEventType, Action<DomainEvent> subscribeMethod);
        void SubscribeFrom(Type domainEventType, long version, Action<DomainEvent> subscribeMethod);
        IEventStoreFacade Include<T>(string nameOfProperty) where T : Entity;
    }
}