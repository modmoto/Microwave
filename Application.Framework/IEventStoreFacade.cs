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
        Task<EventStoreResult<T>> LoadAsync<T>(Guid commandEntityId) where T : new();
        Task<EventStoreResult<IEnumerable<DomainEvent>>> GetEvents(Guid entityId = default(Guid), int from = 0, int to = 100);
        void SubscribeFrom(Type domainEventType, long version, Action<DomainEvent> subscribeMethod);
        Task<long> GetLastProcessedVersion(IEventHandler eventHandler, string eventName);
        Task SaveLastProcessedVersion(IEventHandler eventHandler, DomainEvent prozessedEvent, long lastProcessedVersion);
    }
}