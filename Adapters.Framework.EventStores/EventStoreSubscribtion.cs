using System;
using System.Threading.Tasks;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using Domain.Framework;
using EventStore.ClientAPI;

namespace Adapters.Framework.EventStores
{
    public class EventStoreSubscribtion : IEventStoreSubscribtion
    {
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly DomainEventConverter _eventConverter;
        private readonly EventStoreConfig _eventStoreConfig;

        public EventStoreSubscribtion(IEventStoreConnection eventStoreConnection,
            EventStoreConfig eventStoreConfig,
            DomainEventConverter eventConverter)
        {
            _eventStoreConnection = eventStoreConnection;
            _eventConverter = eventConverter;
            _eventStoreConfig = eventStoreConfig;
        }

        public void SubscribeFrom(string domainEventType, long version, Func<DomainEvent, StreamVersion, Task> subscribeMethod)
        {
            _eventStoreConnection.SubscribeToStreamFrom($"{_eventStoreConfig.ReadStream}-{domainEventType}",
                version,
                new CatchUpSubscriptionSettings(int.MaxValue, 100, true, true),
                (subscription, resolvedEvent) =>
                {
                    if (resolvedEvent.Event == null) return;
                    var streamVersion = new StreamVersion(resolvedEvent.OriginalEventNumber);
                    var domainEvent = _eventConverter.Deserialize(resolvedEvent);
                    subscribeMethod.Invoke(domainEvent, streamVersion);
                });
        }
    }
}