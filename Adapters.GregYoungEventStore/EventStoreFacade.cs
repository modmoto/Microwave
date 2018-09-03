using System.Collections.Generic;
using System.Threading.Tasks;
using Application.Framework;
using Domain.Framework;

namespace Adapters.GregYoungEventStore
{
    public class EventStoreFacade<T> : IDomainEventPersister
    {
        private readonly IEventStoreConfig _eventStoreConfig;

        public EventStoreFacade(IEventStoreConfig eventStoreConfig)
        {
            _eventStoreConfig = eventStoreConfig;
        }

        public Task Save(IEnumerable<DomainEvent> querry)
        {

        }

        Task<IEnumerable<DomainEvent>> IObjectPersister<IEnumerable<DomainEvent>>.GetAsync()
        {
            return GetAsync();
        }
    }

    public interface IEventStoreConfig
    {
    }

    var conn = EventStoreConnection.Create(new Uri("tcp://admin:changeit@localhost:1113"), "MyTestCon");
////
//            conn.ConnectAsync().Wait();
//            var myteststream = "UserEvents";
//            var userCreatedEvent = new UserCreatedEvent(5, "Peter", "Verscrhieben");
//            var userNameChangedEvent = new UserNameChangedEvent(5, "Simon1", "Heiss");
//            var userNameChangedEvent2 = new UserNameChangedEvent(5, "Rahel", "Ludin");
//            string serializeObject = JsonConvert.SerializeObject(userCreatedEvent);
//            string serializeObject2 = JsonConvert.SerializeObject(userNameChangedEvent);
//            string serializeObject3 = JsonConvert.SerializeObject(userNameChangedEvent2);
//            conn.AppendToStreamAsync(myteststream, ExpectedVersion.Any,
//                new EventData(Guid.NewGuid(), nameof(UserCreatedEvent), true, Encoding.UTF8.GetBytes(serializeObject),
//                    null)).Wait();
//            conn.AppendToStreamAsync(myteststream, ExpectedVersion.Any,
//                new EventData(Guid.NewGuid(), nameof(UserNameChangedEvent), true,
//                    Encoding.UTF8.GetBytes(serializeObject2), null)).Wait();
//            conn.AppendToStreamAsync(myteststream, ExpectedVersion.Any,
//                new EventData(Guid.NewGuid(), nameof(UserNameChangedEvent), true,
//                    Encoding.UTF8.GetBytes(serializeObject3), null)).Wait();
//
//            var readEvents = conn.ReadStreamEventsForwardAsync(myteststream, 0, 15, true).Result;
//            foreach (var evt in readEvents.Events)
//                Console.WriteLine(Encoding.UTF8.GetString(evt.Event.Data));

            var projectionsManager = new ProjectionsManager(new ConsoleLogger(),
                new IPEndPoint(new IPAddress(new byte[] {0x7F, 0x00, 0x00, 0x01}), 2113), new TimeSpan(1, 0, 0, 0));

            var result = projectionsManager.GetResultAsync("User5").Result;
            var deserializeObject = JsonConvert.DeserializeObject<User>(result);
}