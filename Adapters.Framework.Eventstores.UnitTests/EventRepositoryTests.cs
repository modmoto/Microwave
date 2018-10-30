using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adapters.Framework.EventStores;
using Adapters.Framework.Subscriptions;
using Adapters.Json.ObjectPersistences;
using Application.Framework.Exceptions;
using Application.Framework.Results;
using Domain.Framework;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;

namespace Adapters.Framework.Eventstores.UnitTests
{
    public class EventRepositoryTests
    {
        [Test]
        public async Task AddAndLoadEvents()
        {
            var options = new DbContextOptionsBuilder<EventStoreWriteContext>()
                .UseInMemoryDatabase("AddEvents")
                .Options;

            var eventStoreContext = new EventStoreWriteContext(options);

            var eventRepository = new EntityStreamRepository(new ObjectConverter(), eventStoreContext);

            var newGuid = Guid.NewGuid();
            var events = new List<DomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};
            var res = await eventRepository.AppendAsync(events, -1);
            res.Check();

            var loadEventsByEntity = await eventRepository.LoadEventsByEntity(newGuid);
            Assert.AreEqual(2, loadEventsByEntity.Value.Count());
            Assert.AreEqual(0, loadEventsByEntity.Value.ToList()[0].Version);
            Assert.AreEqual(newGuid, loadEventsByEntity.Value.ToList()[0].EntityId);
            Assert.AreEqual(1, loadEventsByEntity.Value.ToList()[1].Version);
        }

        [Test]
        public async Task LoadDomainEvents_IdAndStuffIsSetCorreclty()
        {
            var options = new DbContextOptionsBuilder<EventStoreWriteContext>()
                .UseInMemoryDatabase("LoadDomainEvents_IdAndStuffIsSetCorreclty")
                .Options;

            var eventStoreContext = new EventStoreWriteContext(options);

            var eventRepository = new EntityStreamRepository(new ObjectConverter(), eventStoreContext);

            var newGuid = Guid.NewGuid();
            var testEvent1 = new TestEvent1(newGuid);
            var testEvent2 = new TestEvent2(newGuid);
            var events = new List<DomainEvent> { testEvent1, testEvent2};

            var res = await eventRepository.AppendAsync(events, -1);
            res.Check();

            var loadEventsByEntity = await eventRepository.LoadEventsByEntity(newGuid);
            Assert.AreEqual(2, loadEventsByEntity.Value.Count());
            Assert.AreEqual(0, loadEventsByEntity.Value.ToList()[0].Version);
            Assert.AreEqual(1, loadEventsByEntity.Value.ToList()[1].Version);
            Assert.AreEqual(newGuid, loadEventsByEntity.Value.ToList()[0].EntityId);
            Assert.AreEqual(1, loadEventsByEntity.Value.ToList()[1].Version);
        }

        [Test]
        public async Task LoadDomainEvents_IdIsNotRenewed()
        {
            var options = new DbContextOptionsBuilder<EventStoreWriteContext>()
                .UseInMemoryDatabase("LoadDomainEvents_IdIsNotRenewed")
                .Options;

            var eventStoreContext = new EventStoreWriteContext(options);

            var eventRepository = new EntityStreamRepository(new ObjectConverter(), eventStoreContext);

            var newGuid = Guid.NewGuid();
            var testEvent1 = new TestEvent1(newGuid);
            var testEvent2 = new TestEvent2(newGuid);
            var events = new List<DomainEvent> { testEvent1, testEvent2};

            var res = await eventRepository.AppendAsync(events, -1);
            res.Check();

            var loadEventsByEntity = await eventRepository.LoadEventsByEntity(newGuid);

            var domainEventId = loadEventsByEntity.Value.ToList()[0].DomainEventId;
            var domainEventId2 = loadEventsByEntity.Value.ToList()[1].DomainEventId;

            var loadEventsByEntityAgain = await eventRepository.LoadEventsByEntity(newGuid);

            Assert.AreEqual(domainEventId, loadEventsByEntityAgain.Value.ToList()[0].DomainEventId);
            Assert.AreEqual(domainEventId2, loadEventsByEntityAgain.Value.ToList()[1].DomainEventId);
        }

        [Test]
        public async Task AddAndLoadEventsConcurrent()
        {
            var options = new DbContextOptionsBuilder<EventStoreWriteContext>()
                .UseInMemoryDatabase("AddAndLoadEventsConcurrent")
                .Options;

            var eventStoreContext = new EventStoreWriteContext(options);

            var eventRepository = new EntityStreamRepository(new ObjectConverter(), eventStoreContext);

            var newGuid = Guid.NewGuid();
            var events = new List<DomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};
            var events2 = new List<DomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};

            var t1 = eventRepository.AppendAsync(events, -1);
            var t2 = eventRepository.AppendAsync(events2, -1);

            var allResults = await Task.WhenAll(t1, t2);
            var concurrencyException = Assert.Throws<ConcurrencyException>(() => CheckAllResults(allResults));
            var concurrencyExceptionMessage = concurrencyException.Message;
            Assert.AreEqual("Concurrency fraud detected, could not update database. ExpectedVersion: -1, ActualVersion: 1", concurrencyExceptionMessage);
        }

        [Test]
        public async Task Context_DoubleKeyException()
        {
            var options = new DbContextOptionsBuilder<EventStoreWriteContext>()
                .UseInMemoryDatabase("Context_DoubleKeyException")
                .Options;

            var eventStoreContext = new EventStoreWriteContext(options);

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
            var options = new DbContextOptionsBuilder<EventStoreWriteContext>()
                .UseInMemoryDatabase("AddAndLoadEventsByTimeStapmp")
                .Options;

            var eventStoreContext = new EventStoreWriteContext(options);

            var eventRepository = new EntityStreamRepository(new ObjectConverter(), eventStoreContext);

            var newGuid = Guid.NewGuid();
            var events = new List<DomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent1(newGuid), new TestEvent2(newGuid)};

            await eventRepository.AppendAsync(events, -1);

            var result = await eventRepository.LoadEventsSince();
            Assert.AreEqual(4, result.Value.Count());
            Assert.AreEqual(0, result.Value.ToList()[0].Version);
            Assert.AreEqual(newGuid, result.Value.ToList()[0].EntityId);
        }

        [Test]
        public async Task AddAndLoadEventsByTimeStamp_SavedAsType()
        {
            var options = new DbContextOptionsBuilder<EventStoreWriteContext>()
                .UseInMemoryDatabase("AddAndLoadEventsByTimeStapmp_SavedAsType")
                .Options;

            var eventStoreContext = new EventStoreWriteContext(options);

            var optionsRead = new DbContextOptionsBuilder<EventStoreReadContext>()
                .UseInMemoryDatabase("AddAndLoadEventsByTimeStapmp_SavedAsTypeRead")
                .Options;

            var eventStoreReadContext = new EventStoreReadContext(optionsRead);

            var eventRepository = new EntityStreamRepository(new ObjectConverter(), eventStoreContext);
            var typeProjectionRepository = new TypeProjectionRepository(new ObjectConverter(), eventStoreReadContext);

            var newGuid = Guid.NewGuid();
            var domainEvent = new TestEvent1(newGuid);

            await eventRepository.AppendAsync(new List<DomainEvent> { domainEvent }, -1);
            await typeProjectionRepository.AppendToTypeStream(domainEvent);

            var result = await typeProjectionRepository.LoadEventsByTypeAsync(typeof(TestEvent1).Name);
            Assert.AreEqual(1, result.Value.Count());
            Assert.AreEqual(0, result.Value.ToList()[0].Version);
            Assert.AreEqual(newGuid, result.Value.ToList()[0].EntityId);
            Assert.AreEqual(typeof(TestEvent1), result.Value.ToList()[0].GetType());
        }

        [Test]
        public async Task AddEvents_RunTypeProjection()
        {
            var options = new DbContextOptionsBuilder<EventStoreWriteContext>()
                .UseInMemoryDatabase("AddEvents_RunTypeProjection")
                .Options;

            var options2 = new DbContextOptionsBuilder<SubscriptionContext>()
                .UseInMemoryDatabase("AddEvents_RunTypeProjectionSubs")
                .Options;

            var eventStoreContext = new EventStoreWriteContext(options);

            var optionsRead = new DbContextOptionsBuilder<EventStoreReadContext>()
                .UseInMemoryDatabase("AddEvents_RunTypeProjectionReadRead")
                .Options;

            var eventStoreReadContext = new EventStoreReadContext(optionsRead);

            var eventRepository = new EntityStreamRepository(new ObjectConverter(), eventStoreContext);
            var typeProjectionRepository = new TypeProjectionRepository(new ObjectConverter(), eventStoreReadContext);
            var overallProjectionRepository = new OverallProjectionRepository(typeProjectionRepository);

            var newGuid = Guid.NewGuid();
            var events = new List<DomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent1(newGuid), new TestEvent2(newGuid)};

            await eventRepository.AppendAsync(events, -1);
            var versionRepo = new VersionRepository(new SubscriptionContext(options2));
            var typeProjectionHandler = new TypeProjectionHandler(typeProjectionRepository, eventRepository, versionRepo);
            var projectionHandler = new ProjectionHandler(overallProjectionRepository, eventRepository, versionRepo);

            await projectionHandler.Update();
            await typeProjectionHandler.Update();

            var result = await typeProjectionRepository.LoadEventsByTypeAsync(typeof(TestEvent1).Name);

            Assert.AreEqual(2, result.Value.Count());
            Assert.AreEqual(0, result.Value.ToList()[0].Version);
            Assert.AreEqual(newGuid, result.Value.ToList()[0].EntityId);
            Assert.AreEqual(typeof(TestEvent1), result.Value.ToList()[0].GetType());
        }

        private static void CheckAllResults(Result[] whenAll)
        {
            foreach (var result in whenAll)
            {
                result.Check();
            }
        }
    }

    public class TestEvent1 : DomainEvent
    {
        public TestEvent1(Guid entityId) : base(entityId)
        {
        }
    }

    public class TestEvent2 : DomainEvent
    {
        public TestEvent2(Guid entityId) : base(entityId)
        {
        }
    }
}