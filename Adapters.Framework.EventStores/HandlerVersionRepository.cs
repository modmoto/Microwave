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

        private readonly Object _fileLock = new Object();

        public async Task<long> GetLastProcessedVersion(IEventHandler eventHandler, string eventName)
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

        public void IncrementProcessedVersion(IEventHandler eventHandler, DomainEvent prozessedEvent)
        {
            lock (_fileLock)
            {
                var streamName = $"{_eventStoreConfig.ProcessedEventCounterStream}-{eventHandler.GetType().Name}-{prozessedEvent.GetType().Name}";
                var streamEventsSlice = _eventStoreConnection.ReadStreamEventsBackwardAsync(
                    streamName, StreamPosition.End, 1, true).Result;
                var lastProcessedVersion = 0L;
                var streamVersion = -1L;
                if (streamEventsSlice.Events.Length != 0)
                {
                    var resolvedEvent = streamEventsSlice.Events.First();
                    var eventData = Encoding.UTF8.GetString(resolvedEvent.Event.Data);
                    var eventMarker = JsonConvert.DeserializeObject<LastProcessedEventMarker>(eventData);
                    lastProcessedVersion = eventMarker.LastProcessedVersion;
                    streamVersion = streamEventsSlice.LastEventNumber;
                }

                var lastProcessedEventMarker = new LastProcessedEventMarker(lastProcessedVersion + 1, prozessedEvent.DomainEventId);
                var serializedEvent = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(lastProcessedEventMarker));
                var processedEvent = new EventData(Guid.NewGuid(),
                    streamName, true, serializedEvent,
                    new byte[] { });
                _eventStoreConnection.AppendToStreamAsync(streamName, streamVersion, processedEvent).Wait();
            }
        }

    }
}