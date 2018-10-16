using System;
using System.Threading.Tasks;
using Adapters.Json.ObjectPersistences;
using Application.Framework;
using Domain.Framework;
using EventStore.ClientAPI;

namespace Adapters.Framework.EventStores
{
    public class EventStoreSubscription : IEventStoreSubscription
    {
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly DomainEventConverter _eventConverter;
        private readonly EventStoreConfig _eventStoreConfig;

        public EventStoreSubscription(IEventStoreConnection eventStoreConnection,
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
                new CatchUpSubscriptionSettings(int.MaxValue, 1000, true, true),
                (subscription, resolvedEvent) =>
                {
                    if (resolvedEvent.Event == null) return Task.CompletedTask;
                    var streamVersion = new StreamVersion(resolvedEvent.OriginalEventNumber);
                    var domainEvent = _eventConverter.Deserialize(resolvedEvent);
                    return Task.FromResult(subscribeMethod.Invoke(domainEvent, streamVersion));
                }, subscriptionDropped: dropped);
        }

        private void dropped(EventStoreCatchUpSubscription arg1, SubscriptionDropReason arg2, Exception arg3)
        {
            Console.WriteLine("lul");
        }
    }
}