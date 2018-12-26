using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application.Exceptions;
using Microwave.Application.Results;
using Microwave.Domain;
using Microwave.EventStores;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;

namespace Microwave.Eventstores.UnitTests
{
    [TestClass]
    public class EventRepositoryTests : IntegrationTests
    {
        [TestMethod]
        public async Task AddAndLoadEvents()
        {
            var eventRepository = new EventRepository(new EventDatabase(Database));

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent3(newGuid, "TestName")};
            var res = await eventRepository.AppendAsync(events, 0);
            res.Check();

            var loadEventsByEntity = await eventRepository.LoadEventsByEntity(newGuid);
            Assert.AreEqual(3, loadEventsByEntity.Value.Count());
            Assert.AreEqual(1, loadEventsByEntity.Value.ToList()[0].Version);
            var domainEvent = (TestEvent3) loadEventsByEntity.Value.ToList()[2].DomainEvent;
            Assert.AreEqual(newGuid.Id, domainEvent.EntityId.Id);
            Assert.AreEqual("TestName", domainEvent.Name);
            Assert.AreEqual(2, loadEventsByEntity.Value.ToList()[1].Version);
        }

        // this is because of mongodb, constructor has to be named the same
        [TestMethod]
        public async Task AddAndLoadEvents_ParamCalledWrong()
        {
            var eventRepository = new EventRepository(new EventDatabase(Database));

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent_ParameterCalledWrong(newGuid)};
            await eventRepository.AppendAsync(events, 0);

            var loadEventsByEntity = await eventRepository.LoadEventsByEntity(newGuid);
            Assert.AreEqual(1, loadEventsByEntity.Value.Count());
            Assert.AreEqual(null, loadEventsByEntity.Value.ToList()[0].DomainEvent.EntityId);
        }

        [TestMethod]
        public async Task LoadDomainEvents_IdAndStuffIsSetCorreclty()
        {
            var eventRepository = new EventRepository(new EventDatabase(Database));

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var testEvent1 = new TestEvent1(newGuid);
            var testEvent2 = new TestEvent2(newGuid);
            var events = new List<IDomainEvent> { testEvent1, testEvent2};

            var res = await eventRepository.AppendAsync(events, 0);
            res.Check();

            var loadEventsByEntity = await eventRepository.LoadEventsByEntity(newGuid);
            Assert.AreEqual(2, loadEventsByEntity.Value.Count());
            Assert.AreNotEqual(0, loadEventsByEntity.Value.ToList()[0].Created);
            Assert.AreNotEqual(0, loadEventsByEntity.Value.ToList()[1].Created);
            Assert.AreEqual(1, loadEventsByEntity.Value.ToList()[0].Version);
            Assert.AreEqual(2, loadEventsByEntity.Value.ToList()[1].Version);
            Assert.IsTrue(newGuid.Equals(loadEventsByEntity.Value.ToList()[0].DomainEvent.EntityId));
        }

        [TestMethod]
        public async Task AddAndLoadEventsConcurrent()
        {
            var eventRepository = new EventRepository(new EventDatabase(Database));

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};
            var events2 = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};

            var t1 = eventRepository.AppendAsync(events, 0);
            var t2 = eventRepository.AppendAsync(events2, 0);

            var allResults = await Task.WhenAll(t1, t2);
            var concurrencyException = Assert.ThrowsException<ConcurrencyViolatedException>(() => CheckAllResults(allResults));
            var concurrencyExceptionMessage = concurrencyException.Message;
            Assert.AreEqual("Concurrency fraud detected, could not update database. ExpectedVersion: 0, ActualVersion: 2", concurrencyExceptionMessage);

            var loadEvents = await eventRepository.LoadEvents();
            Assert.AreEqual(2, loadEvents.Value.Count());
        }

        [TestMethod]
        public async Task AddEmptyEventListt()
        {
            var eventRepository = new EventRepository(new EventDatabase(Database));

            var appendAsync = await eventRepository.AppendAsync(new List<IDomainEvent>(), 0);
            appendAsync.Check();
        }

        [TestMethod]
        public async Task LoadEventsByTypeAsync()
        {
            var eventRepository = new EventRepository(new EventDatabase(Database));

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent2(newGuid)};

            await eventRepository.AppendAsync(events, 0);
            var eventsLoaded = await eventRepository.LoadEventsByTypeAsync(typeof(TestEvent2).Name, 0);

            Assert.AreEqual(2, eventsLoaded.Value.Count());
        }

        [TestMethod]
        public async Task Context_DoubleKeyException()
        {
            var entityId = GuidIdentity.Create(Guid.NewGuid()).ToString();
            var domainEventDbo = new DomainEventDbo
            {
                Key = new DomainEventKey
                {
                    EntityId = entityId,
                    Version = 1
                }
            };

            var domainEventDbo2 = new DomainEventDbo
            {
                Key = new DomainEventKey
                {
                    EntityId = entityId,
                    Version = 1
                }
            };

            var mongoCollection = Database.GetCollection<DomainEventDbo>("DomainEventDbos");
            await mongoCollection.InsertOneAsync(domainEventDbo);
            await Assert.ThrowsExceptionAsync<MongoWriteException>(async () => await mongoCollection.InsertOneAsync(domainEventDbo2));
        }

        [TestMethod]
        public async Task AddAndLoadEventsByTimeStamp()
        {
            var eventRepository = new EventRepository(new EventDatabase(Database));

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent1(newGuid), new TestEvent2(newGuid)};

            await eventRepository.AppendAsync(events, 0);

            var result = await eventRepository.LoadEvents();
            Assert.AreEqual(4, result.Value.Count());
            Assert.AreEqual(1, result.Value.ToList()[0].Version);
            Assert.AreEqual(newGuid.Id, result.Value.ToList()[0].DomainEvent.EntityId.Id);
        }

        [TestMethod]
        public async Task AddEvents_FirstEventAfterCreationHasWrongRowVersionBug()
        {
            var eventRepository = new EventRepository(new EventDatabase(Database));

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent1(newGuid), new TestEvent2(newGuid)};

            await eventRepository.AppendAsync(events, 0);

            var result = await eventRepository.LoadEvents();
            Assert.AreEqual(4, result.Value.Count());
            Assert.AreEqual(1, result.Value.ToList()[0].Version);
            Assert.AreEqual(2, result.Value.ToList()[1].Version);
            Assert.AreEqual(3, result.Value.ToList()[2].Version);
            Assert.AreEqual(newGuid.Id, result.Value.ToList()[0].DomainEvent.EntityId.Id);
        }

        [TestMethod]
        public async Task AddEvents_VersionTooHigh()
        {
            var eventRepository = new EventRepository(new EventDatabase(Database));

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent1(newGuid), new TestEvent2(newGuid)};

            var result = await eventRepository.AppendAsync(events, 1);

            Assert.IsTrue(result.Is<ConcurrencyError>());
        }

        [TestMethod]
        public async Task AddEvents_VersionWayTooHigh()
        {
            var eventRepository = new EventRepository(new EventDatabase(Database));

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent1(newGuid), new TestEvent2(newGuid)};

            var result = await eventRepository.AppendAsync(events, 5);
            Assert.IsTrue(result.Is<ConcurrencyError>());
        }

        [TestMethod]
        public async Task AddAndLoadEventsByTimeStamp_SavedAsType()
        {
            var eventRepository = new EventRepository(new EventDatabase(Database));

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var domainEvent = new TestEvent1(newGuid);

            await eventRepository.AppendAsync(new List<IDomainEvent> { domainEvent }, 0);

            var result = await eventRepository.LoadEventsByTypeAsync(typeof(TestEvent1).Name, 0);
            Assert.AreEqual(1, result.Value.Count());
            Assert.AreEqual(1, result.Value.ToList()[0].Version);
            Assert.AreEqual(newGuid.Id, result.Value.ToList()[0].DomainEvent.EntityId.Id);
            Assert.AreEqual(typeof(TestEvent1), result.Value.ToList()[0].DomainEvent.GetType());
        }

        [TestMethod]
        public async Task AddEvents_IdSet()
        {
            var eventRepository = new EventRepository(new EventDatabase(Database));

            var testEvent1 = new TestEvent1(GuidIdentity.Create(Guid.NewGuid()));
            await eventRepository.AppendAsync(new[] {testEvent1}, 0);

            var result = await eventRepository.LoadEventsByEntity(testEvent1.EntityId);
            var domainEvent = result.Value.Single().DomainEvent;

            Assert.IsTrue(domainEvent.EntityId.Equals(testEvent1.EntityId));
        }

        [TestMethod]
        public async Task AddEvents_IdOfTypeSet()
        {
            var eventRepository = new EventRepository(new EventDatabase(Database));

            var testEvent1 = new TestEvent1(GuidIdentity.Create(Guid.NewGuid()));
            await eventRepository.AppendAsync(new List<IDomainEvent> { testEvent1 }, 0);

            var result = await eventRepository.LoadEventsByTypeAsync(testEvent1.GetType().Name, 0);
            var domainEvent = result.Value.Single().DomainEvent;

            Assert.IsTrue(domainEvent.EntityId.Equals(testEvent1.EntityId));
        }

        [TestMethod]
        public async Task AddEvents_RunTypeProjection()
        {
            var eventRepository = new EventRepository(new EventDatabase(Database));

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent1(newGuid), new TestEvent2(newGuid)};

            await eventRepository.AppendAsync(events, 0);

            var result = await eventRepository.LoadEventsByTypeAsync(typeof(TestEvent1).Name, 0);

            Assert.AreEqual(2, result.Value.Count());
            Assert.AreEqual(1, result.Value.ToList()[0].Version);
            Assert.AreEqual(3, result.Value.ToList()[1].Version);
            Assert.IsTrue(newGuid.Equals(result.Value.ToList()[0].DomainEvent.EntityId));
            Assert.AreEqual(typeof(TestEvent1), result.Value.ToList()[0].DomainEvent.GetType());
        }

        private static void CheckAllResults(Result[] whenAll)
        {
            foreach (var result in whenAll)
            {
                result.Check();
            }
        }
    }

    public class TestEvent1 : IDomainEvent
    {
        public TestEvent1(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }

    public class TestEvent_ParameterCalledWrong : IDomainEvent
    {
        public TestEvent_ParameterCalledWrong(Identity notCalledEntityId)
        {
            EntityId = notCalledEntityId;
        }

        public Identity EntityId { get; }
    }

    public class TestEvent2 : IDomainEvent
    {
        public TestEvent2(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }

    public class TestEvent3 : IDomainEvent
    {
        public TestEvent3(Identity entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }

        public Identity EntityId { get; }
        public string Name { get; }
    }
}