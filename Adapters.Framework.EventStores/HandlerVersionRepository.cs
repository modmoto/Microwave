using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Framework;
using EventStore.ClientAPI;
using Newtonsoft.Json;

namespace Adapters.Framework.EventStores
{
    public class HandlerVersionRepository : IHandlerVersionRepository
    {
        private readonly IEventStoreConnection _eventStoreConnection;
        private readonly EventStoreConfig _eventStoreConfig;

        public HandlerVersionRepository(IEventStoreConnection eventStoreConnection, EventStoreConfig eventStoreConfig)
        {
            _eventStoreConnection = eventStoreConnection;
            _eventStoreConfig = eventStoreConfig;
        }

        public async Task<long> GetLastProcessedVersion(IReactiveEventHandler eventHandler, string eventName)
        {
            var streamName = $"{_eventStoreConfig.ProcessedEventCounterStream}-{eventHandler.GetType().Name}-{eventName}";
            var streamEventsSlice = await _eventStoreConnection.ReadStreamEventsBackwardAsync(
                streamName, StreamPosition.End, 1, true);
            if (streamEventsSlice.Events.Length == 0) return 0;
            var resolvedEvent = streamEventsSlice.Events.First();
            var eventData = Encoding.UTF8.GetString(resolvedEvent.Event.Data);
            var eventMarker = JsonConvert.DeserializeObject<LastProcessedEventMarker>(eventData);
            return eventMarker.LastProcessedVersion;
        }

        public void IncrementProcessedVersion(IReactiveEventHandler eventHandler, DomainEvent prozessedEvent,
            StreamVersion streamVersion)
        {
            var streamName = $"{_eventStoreConfig.ProcessedEventCounterStream}-{eventHandler.GetType().Name}-{prozessedEvent.GetType().Name}";
            var lastProcessedEventMarker = new LastProcessedEventMarker(streamVersion.EventNumber, prozessedEvent.DomainEventId);
            var serializedEvent = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(lastProcessedEventMarker));
            var processedEvent = new EventData(Guid.NewGuid(),
                nameof(LastProcessedEventMarker), true, serializedEvent,
                new byte[] { });
            _eventStoreConnection.AppendToStreamAsync(streamName, ExpectedVersion.Any, processedEvent).Wait();
        }

    }
}