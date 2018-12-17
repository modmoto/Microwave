using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application.Exceptions;
using Microwave.Application.Results;
using Microwave.Domain;
using Microwave.EventStores;
using Microwave.ObjectPersistences;
using Mongo2Go;
using MongoDB.Driver;

namespace Microwave.Eventstores.UnitTests
{
    [TestClass]
    public class EventRepositoryTests
    {
        [TestMethod]
        public async Task AddAndLoadEvents()
        {
            var runner = MongoDbRunner.Start("AddAndLoadEvents");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("AddAndLoadEvents");

            var eventRepository = new EventRepository(database, new DomainEventDeserializer(new JSonHack()), new ObjectConverter());

            var newGuid = Guid.NewGuid();
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};
            var res = await eventRepository.AppendAsync(events, 0);
            res.Check();

            var loadEventsByEntity = await eventRepository.LoadEventsByEntity(newGuid);
            Assert.AreEqual(2, loadEventsByEntity.Value.Count());
            Assert.AreEqual(1, loadEventsByEntity.Value.ToList()[0].Version);
            Assert.AreEqual(newGuid, loadEventsByEntity.Value.ToList()[0].DomainEvent.EntityId);
            Assert.AreEqual(2, loadEventsByEntity.Value.ToList()[1].Version);

            client.DropDatabase("AddAndLoadEvents");
            runner.Dispose();
        }

        [TestMethod]
        public async Task LoadDomainEvents_IdAndStuffIsSetCorreclty()
        {
            var runner = MongoDbRunner.Start("LoadDomainEvents_IdAndStuffIsSetCorreclty");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("LoadDomainEvents_IdAndStuffIsSetCorreclty");

            var eventRepository = new EventRepository(database, new DomainEventDeserializer(new JSonHack()), new ObjectConverter());

            var newGuid = Guid.NewGuid();
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
            Assert.AreEqual(newGuid, loadEventsByEntity.Value.ToList()[0].DomainEvent.EntityId);

            client.DropDatabase("LoadDomainEvents_IdAndStuffIsSetCorreclty");
            runner.Dispose();
        }

        [TestMethod]
        public async Task AddAndLoadEventsConcurrent()
        {
            var runner = MongoDbRunner.Start("AddAndLoadEventsConcurrent");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("AddAndLoadEventsConcurrent");
            client.DropDatabase("AddAndLoadEventsConcurrent");

            var eventRepository = new EventRepository(database, new DomainEventDeserializer(new JSonHack()), new ObjectConverter());

            var newGuid = Guid.NewGuid();
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

            runner.Dispose();
        }

        [TestMethod]
        public async Task AddEmptyEventListt()
        {
            var runner = MongoDbRunner.Start("AddEmptyEventListt");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("AddEmptyEventListt");

            var eventRepository = new EventRepository(database, new DomainEventDeserializer(new JSonHack()), new ObjectConverter());

            var appendAsync = await eventRepository.AppendAsync(new List<IDomainEvent>(), 0);
            appendAsync.Check();

            client.DropDatabase("AddEmptyEventListt");
            runner.Dispose();
        }

        [TestMethod]
        public async Task Context_DoubleKeyException()
        {
            var runner = MongoDbRunner.Start("Context_DoubleKeyException");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("Context_DoubleKeyException");

            var entityId = Guid.NewGuid().ToString();
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

            var mongoCollection = database.GetCollection<DomainEventDbo>("DomainEventDbos");
            await mongoCollection.InsertOneAsync(domainEventDbo);
            await Assert.ThrowsExceptionAsync<MongoWriteException>(async () => await mongoCollection.InsertOneAsync(domainEventDbo2));

            client.DropDatabase("Context_DoubleKeyException");
            runner.Dispose();
        }

        [TestMethod]
        public async Task AddAndLoadEventsByTimeStamp()
        {
            var runner = MongoDbRunner.Start("AddAndLoadEventsByTimeStamp");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("AddAndLoadEventsByTimeStamp");

            var eventRepository = new EventRepository(database, new DomainEventDeserializer(new JSonHack()), new ObjectConverter());

            var newGuid = Guid.NewGuid();
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent1(newGuid), new TestEvent2(newGuid)};

            await eventRepository.AppendAsync(events, 0);

            var result = await eventRepository.LoadEvents();
            Assert.AreEqual(4, result.Value.Count());
            Assert.AreEqual(1, result.Value.ToList()[0].Version);
            Assert.AreEqual(newGuid, result.Value.ToList()[0].DomainEvent.EntityId);

            client.DropDatabase("AddAndLoadEventsByTimeStapmp");
            runner.Dispose();
        }

        [TestMethod]
        public async Task AddEvents_FirstEventAfterCreationHasWrongRowVersionBug()
        {
            var runner = MongoDbRunner.Start("AddEvents_FirstEventAfterCreationHasWrongRowVersionBug");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("AddEvents_FirstEventAfterCreationHasWrongRowVersionBug");

            var eventRepository = new EventRepository(database, new DomainEventDeserializer(new JSonHack()), new ObjectConverter());

            var newGuid = Guid.NewGuid();
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent1(newGuid), new TestEvent2(newGuid)};

            await eventRepository.AppendAsync(events, 0);

            var result = await eventRepository.LoadEvents();
            Assert.AreEqual(4, result.Value.Count());
            Assert.AreEqual(1, result.Value.ToList()[0].Version);
            Assert.AreEqual(2, result.Value.ToList()[1].Version);
            Assert.AreEqual(3, result.Value.ToList()[2].Version);
            Assert.AreEqual(newGuid, result.Value.ToList()[0].DomainEvent.EntityId);

            client.DropDatabase("AddEvents_FirstEventAfterCreationHasWrongRowVersionBug");
            runner.Dispose();
        }

        [TestMethod]
        public async Task AddEvents_VersionTooHigh()
        {
            var runner = MongoDbRunner.Start("AddEvents_VersionTooHigh");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("AddEvents_VersionTooHigh");

            var eventRepository = new EventRepository(database, new DomainEventDeserializer(new JSonHack()), new ObjectConverter());

            var newGuid = Guid.NewGuid();
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent1(newGuid), new TestEvent2(newGuid)};

            var result = await eventRepository.AppendAsync(events, 1);

            Assert.IsTrue(result.Is<ConcurrencyError>());

            client.DropDatabase("AddEvents_VersionTooHigh");
            runner.Dispose();
        }

        [TestMethod]
        public async Task AddEvents_VersionWayTooHigh()
        {
            var runner = MongoDbRunner.Start("AddEvents_VersionWayTooHigh");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("AddEvents_VersionWayTooHigh");

            var eventRepository = new EventRepository(database, new DomainEventDeserializer(new JSonHack()), new ObjectConverter());

            var newGuid = Guid.NewGuid();
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent1(newGuid), new TestEvent2(newGuid)};

            var result = await eventRepository.AppendAsync(events, 5);
            Assert.IsTrue(result.Is<ConcurrencyError>());

            client.DropDatabase("AddEvents_VersionWayTooHigh");
            runner.Dispose();
        }

        [TestMethod]
        public async Task AddAndLoadEventsByTimeStamp_SavedAsType()
        {
            var runner = MongoDbRunner.Start("AddAndLoadEventsByTimeStamp_SavedAsType");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("AddAndLoadEventsByTimeStamp_SavedAsType");

            var eventRepository = new EventRepository(database, new DomainEventDeserializer(new JSonHack()), new ObjectConverter());

            var newGuid = Guid.NewGuid();
            var domainEvent = new TestEvent1(newGuid);

            await eventRepository.AppendAsync(new List<IDomainEvent> { domainEvent }, 0);

            var result = await eventRepository.LoadEventsByTypeAsync(typeof(TestEvent1).Name, 0);
            Assert.AreEqual(1, result.Value.Count());
            Assert.AreEqual(1, result.Value.ToList()[0].Version);
            Assert.AreEqual(newGuid, result.Value.ToList()[0].DomainEvent.EntityId);
            Assert.AreEqual(typeof(TestEvent1), result.Value.ToList()[0].DomainEvent.GetType());

            client.DropDatabase("AddAndLoadEventsByTimeStapmp_SavedAsType");
            runner.Dispose();
        }

        [TestMethod]
        public async Task AddEvents_IdSet()
        {
            var runner = MongoDbRunner.Start("AddEvents_IdSet");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("AddEvents_IdSet");

            var eventRepository = new EventRepository(database, new DomainEventDeserializer(new JSonHack()), new ObjectConverter());

            var testEvent1 = new TestEvent1(Guid.NewGuid());
            await eventRepository.AppendAsync(new[] {testEvent1}, 0);

            var result = await eventRepository.LoadEventsByEntity(testEvent1.EntityId);
            var domainEvent = result.Value.Single().DomainEvent;

            Assert.AreEqual(domainEvent.EntityId, testEvent1.EntityId);

            client.DropDatabase("AddEvents_IdSet");
            runner.Dispose();
        }

        [TestMethod]
        public async Task AddEvents_IdOfTypeSet()
        {
            var runner = MongoDbRunner.Start("AddEvents_IdOfTypeSet");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("AddEvents_IdOfTypeSet");

            var eventRepository = new EventRepository(database, new DomainEventDeserializer(new JSonHack()), new ObjectConverter());

            var testEvent1 = new TestEvent1(Guid.NewGuid());
            await eventRepository.AppendAsync(new List<IDomainEvent> { testEvent1 }, 0);

            var result = await eventRepository.LoadEventsByTypeAsync(testEvent1.GetType().Name, 0);
            var domainEvent = result.Value.Single().DomainEvent;

            Assert.AreEqual(domainEvent.EntityId, testEvent1.EntityId);

            client.DropDatabase("AddEvents_IdOfTypeSet");
            runner.Dispose();
        }

        [TestMethod]
        public async Task AddEvents_RunTypeProjection()
        {
            var runner = MongoDbRunner.Start("AddEvents_RunTypeProjection");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("AddEvents_RunTypeProjection");
            client.DropDatabase("AddEvents_RunTypeProjection");

            var eventRepository = new EventRepository(database, new DomainEventDeserializer(new JSonHack()), new ObjectConverter());

            var newGuid = Guid.NewGuid();
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent1(newGuid), new TestEvent2(newGuid)};

            await eventRepository.AppendAsync(events, 0);

            var result = await eventRepository.LoadEventsByTypeAsync(typeof(TestEvent1).Name, 0);

            Assert.AreEqual(2, result.Value.Count());
            Assert.AreEqual(1, result.Value.ToList()[0].Version);
            Assert.AreEqual(3, result.Value.ToList()[1].Version);
            Assert.AreEqual(newGuid, result.Value.ToList()[0].DomainEvent.EntityId);
            Assert.AreEqual(typeof(TestEvent1), result.Value.ToList()[0].DomainEvent.GetType());

            runner.Dispose();
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
        public TestEvent1(Guid entityId)
        {
            EntityId = entityId;
        }

        public Guid EntityId { get; }
    }

    public class TestEvent2 : IDomainEvent
    {
        public TestEvent2(Guid entityId)
        {
            EntityId = entityId;
        }

        public Guid EntityId { get; }
    }
}