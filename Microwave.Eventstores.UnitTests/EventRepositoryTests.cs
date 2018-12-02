using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microwave.Application.Exceptions;
using Microwave.Application.Results;
using Microwave.Domain;
using Microwave.EventStores;
using Microwave.ObjectPersistences;
using Microwave.Queries;
using NUnit.Framework;

namespace Microwave.Eventstores.UnitTests
{
    public class EventRepositoryTests
    {
        [Test]
        public async Task AddAndLoadEvents()
        {
            var options = new DbContextOptionsBuilder<EventStoreContext>()
                .UseInMemoryDatabase("AddEvents")
                .Options;

            var eventStoreContext = new EventStoreContext(options);

            var eventRepository = new EntityStreamRepository(new DomainEventDeserializer(new JSonHack()), eventStoreContext, new ObjectConverter());

            var newGuid = Guid.NewGuid();
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};
            var res = await eventRepository.AppendAsync(events, 0);
            res.Check();

            var loadEventsByEntity = await eventRepository.LoadEventsByEntity(newGuid);
            Assert.AreEqual(2, loadEventsByEntity.Value.Count());
            Assert.AreEqual(1, loadEventsByEntity.Value.ToList()[0].Version);
            Assert.AreEqual(newGuid, loadEventsByEntity.Value.ToList()[0].DomainEvent.EntityId);
            Assert.AreEqual(2, loadEventsByEntity.Value.ToList()[1].Version);
        }

        [Test]
        public async Task LoadDomainEvents_IdAndStuffIsSetCorreclty()
        {
            var options = new DbContextOptionsBuilder<EventStoreContext>()
                .UseInMemoryDatabase("LoadDomainEvents_IdAndStuffIsSetCorreclty")
                .Options;

            var eventStoreContext = new EventStoreContext(options);

            var eventRepository = new EntityStreamRepository(new DomainEventDeserializer(new JSonHack()), eventStoreContext, new ObjectConverter());

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
        }

        [Test]
        public async Task AddAndLoadEventsConcurrent()
        {
            var options = new DbContextOptionsBuilder<EventStoreContext>()
                .UseInMemoryDatabase("AddAndLoadEventsConcurrent")
                .Options;

            var eventStoreContext = new EventStoreContext(options);

            var eventRepository = new EntityStreamRepository(new DomainEventDeserializer(new JSonHack()), eventStoreContext, new ObjectConverter());

            var newGuid = Guid.NewGuid();
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};
            var events2 = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};

            var t1 = eventRepository.AppendAsync(events, 0);
            var t2 = eventRepository.AppendAsync(events2, 0);

            var allResults = await Task.WhenAll(t1, t2);
            var concurrencyException = Assert.Throws<ConcurrencyViolatedException>(() => CheckAllResults(allResults));
            var concurrencyExceptionMessage = concurrencyException.Message;
            Assert.AreEqual("Concurrency fraud detected, could not update database. ExpectedVersion: 0, ActualVersion: 2", concurrencyExceptionMessage);
        }

        [Test]
        public async Task Context_DoubleKeyException()
        {
            var options = new DbContextOptionsBuilder<EventStoreContext>()
                .UseInMemoryDatabase("Context_DoubleKeyException")
                .Options;

            var eventStoreContext = new EventStoreContext(options);

            var entityId = Guid.NewGuid().ToString();
            var domainEventDbo = new DomainEventDbo
            {
                EntityId = entityId,
                Version = 1
            };

            var domainEventDbo2 = new DomainEventDbo
            {
                EntityId = entityId,
                Version = 1
            };

            await eventStoreContext.EntityStreams.AddAsync(domainEventDbo);
            Assert.ThrowsAsync<InvalidOperationException>(async () => await eventStoreContext.EntityStreams.AddAsync(domainEventDbo2));
        }

        [Test]
        public async Task AddAndLoadEventsByTimeStamp()
        {
            var options = new DbContextOptionsBuilder<EventStoreContext>()
                .UseInMemoryDatabase("AddAndLoadEventsByTimeStapmp")
                .Options;

            var eventStoreContext = new EventStoreContext(options);

            var eventRepository = new EntityStreamRepository(new DomainEventDeserializer(new JSonHack()), eventStoreContext, new ObjectConverter());

            var newGuid = Guid.NewGuid();
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent1(newGuid), new TestEvent2(newGuid)};

            await eventRepository.AppendAsync(events, 0);

            var result = await eventRepository.LoadEventsSince();
            Assert.AreEqual(4, result.Value.Count());
            Assert.AreEqual(1, result.Value.ToList()[0].Version);
            Assert.AreEqual(newGuid, result.Value.ToList()[0].DomainEvent.EntityId);
        }

        [Test]
        public async Task AddAndLoadEventsByTimeStamp_SavedAsType()
        {
            var options = new DbContextOptionsBuilder<EventStoreContext>()
                .UseInMemoryDatabase("AddAndLoadEventsByTimeStapmp_SavedAsType")
                .Options;

            var eventStoreContext = new EventStoreContext(options);

            var eventRepository = new EntityStreamRepository(new DomainEventDeserializer(new JSonHack()), eventStoreContext, new ObjectConverter());
            var typeProjectionRepository = new TypeProjectionRepository(new ObjectConverter(), new DomainEventDeserializer(new JSonHack()),  eventStoreContext);

            var newGuid = Guid.NewGuid();
            var domainEvent = new TestEvent1(newGuid);

            await eventRepository.AppendAsync(new List<IDomainEvent> { domainEvent }, 0);
            await typeProjectionRepository.AppendToTypeStream(domainEvent);

            var result = await typeProjectionRepository.LoadEventsByTypeAsync(typeof(TestEvent1).Name);
            Assert.AreEqual(1, result.Value.Count());
            Assert.AreEqual(1, result.Value.ToList()[0].Version);
            Assert.AreEqual(newGuid, result.Value.ToList()[0].DomainEvent.EntityId);
            Assert.AreEqual(typeof(TestEvent1), result.Value.ToList()[0].DomainEvent.GetType());
        }

        [Test]
        public async Task AddEvents_IdSet()
        {
            var options = new DbContextOptionsBuilder<EventStoreContext>()
                .UseInMemoryDatabase("AddEvents_IdAndStuffSet")
                .Options;
            var eventStoreContext = new EventStoreContext(options);

            var eventRepository = new EntityStreamRepository(new DomainEventDeserializer(new JSonHack()), eventStoreContext, new ObjectConverter());

            var testEvent1 = new TestEvent1(Guid.NewGuid());
            await eventRepository.AppendAsync(new[] {testEvent1}, 0);

            var result = await eventRepository.LoadEventsByEntity(testEvent1.EntityId);
            var domainEvent = result.Value.Single().DomainEvent;

            Assert.AreEqual(domainEvent.EntityId, testEvent1.EntityId);
        }

        [Test]
        public async Task AddEvents_IdOfTypeSet()
        {
            var options = new DbContextOptionsBuilder<EventStoreContext>()
                .UseInMemoryDatabase("AddEvents_TypeSet")
                .Options;

            var eventStoreContext = new EventStoreContext(options);

            var eventRepository = new TypeProjectionRepository(new ObjectConverter(), new DomainEventDeserializer(new JSonHack()),  eventStoreContext);

            var testEvent1 = new TestEvent1(Guid.NewGuid());
            await eventRepository.AppendToTypeStream(testEvent1);

            var result = await eventRepository.LoadEventsByTypeAsync(testEvent1.GetType().Name);
            var domainEvent = result.Value.Single().DomainEvent;

            Assert.AreEqual(domainEvent.EntityId, testEvent1.EntityId);
        }

        [Test]
        public async Task AddEvents_RunTypeProjection()
        {
            var options = new DbContextOptionsBuilder<EventStoreContext>()
                .UseInMemoryDatabase("AddEvents_RunTypeProjection")
                .Options;

            var eventStoreContext = new EventStoreContext(options);

            var eventRepository = new EntityStreamRepository(new DomainEventDeserializer(new JSonHack()), eventStoreContext, new ObjectConverter());
            var typeProjectionRepository = new TypeProjectionRepository(new ObjectConverter(), new DomainEventDeserializer(new JSonHack()),  eventStoreContext);
            var overallProjectionRepository = new OverallProjectionRepository(typeProjectionRepository);

            var newGuid = Guid.NewGuid();
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent1(newGuid), new TestEvent2(newGuid)};

            await eventRepository.AppendAsync(events, 0);
            var versionRepo = new VersionRepository(eventStoreContext);
            var typeProjectionHandler = new TypeProjectionHandler(typeProjectionRepository, eventRepository, versionRepo);
            var projectionHandler = new ProjectionHandler(overallProjectionRepository, eventRepository, versionRepo);

            await projectionHandler.Update();
            await typeProjectionHandler.Update();

            var result = await typeProjectionRepository.LoadEventsByTypeAsync(typeof(TestEvent1).Name);

            Assert.AreEqual(2, result.Value.Count());
            Assert.AreEqual(1, result.Value.ToList()[0].Version);
            Assert.AreEqual(2, result.Value.ToList()[1].Version);
            Assert.AreEqual(newGuid, result.Value.ToList()[0].DomainEvent.EntityId);
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