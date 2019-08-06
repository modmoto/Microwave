using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Exceptions;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;
using Microwave.Persistence.UnitTestSetupPorts;

namespace Microwave.Persistence.UnitTests.Eventstores
{
    [TestClass]
    public class EventRepositoryTests
    {
        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task AddAndLoadEvents(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

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

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task LoadWithNulls(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var loadEventsByEntity = await eventRepository.LoadEventsByEntity(null);
            Assert.IsTrue(loadEventsByEntity.Is<NotFound>());
            Assert.ThrowsException<NotFoundException>(() => loadEventsByEntity.Value);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task AddAndLoadEvents_Twice(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent3(newGuid, "TestName")};
            var res = await eventRepository.AppendAsync(events, 0);
            var res2 = await eventRepository.AppendAsync(events, 3);
            res.Check();
            res2.Check();

            var loadEventsByEntity = await eventRepository.LoadEventsByEntity(newGuid);
            Assert.AreEqual(6, loadEventsByEntity.Value.Count());
        }

        // this is because of mongodb, constructor has to be named the same
        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task AddAndLoadEvents_ParamCalledWrong(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent_ParameterCalledWrong(newGuid)};
            await eventRepository.AppendAsync(events, 0);

            var loadEventsByEntity = await eventRepository.LoadEventsByEntity(newGuid);
            Assert.AreEqual(1, loadEventsByEntity.Value.Count());
            Assert.AreEqual(null, loadEventsByEntity.Value.ToList()[0].DomainEvent.EntityId);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task LoadDomainEvents_IdAndStuffIsSetCorreclty(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

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
            Assert.IsTrue(newGuid == loadEventsByEntity.Value.ToList()[0].DomainEvent.EntityId);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task AddAndLoadEventsConcurrent(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};
            var events2 = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};

            var t1 = eventRepository.AppendAsync(events, 0);
            var t2 = eventRepository.AppendAsync(events2, 0);

            var allResults = await Task.WhenAll(t1, t2);
            var concurrencyException = Assert.ThrowsException<ConcurrencyViolatedException>(() => CheckAllResults(allResults));
            var concurrencyExceptionMessage = concurrencyException.Message;
            Assert.AreEqual("Concurrency violation detected, could not update database. ExpectedVersion: 0, ActualVersion: 2", concurrencyExceptionMessage);

            var loadEvents = await eventRepository.LoadEvents();
            Assert.AreEqual(2, loadEvents.Value.Count());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task AddAndLoadEventsConcurrent_AfterNormalAdd(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};
            var events2 = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};

            await eventRepository.AppendAsync(events, 0);
            var t1 = eventRepository.AppendAsync(events, 2);
            var t2 = eventRepository.AppendAsync(events2, 2);

            var allResults = await Task.WhenAll(t1, t2);
            Assert.ThrowsException<ConcurrencyViolatedException>(() => CheckAllResults(allResults));

            var loadEvents = await eventRepository.LoadEvents();
            Assert.AreEqual(4, loadEvents.Value.Count());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task AddAndLoadEventsConcurrent_AddAfterwardsAgain(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};
            var events2 = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};

            var t1 = eventRepository.AppendAsync(events, 0);
            var t2 = eventRepository.AppendAsync(events2, 0);

            await Task.WhenAll(t1, t2);

            var res = await eventRepository.AppendAsync(events2, 2);

            Assert.IsTrue(res.Is<Ok>());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task AddAndLoadEventsConcurrent_AddAfterwardsAgain_DifferentRepo(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;
            var eventRepository2 = layerProvider.EventRepository;

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};
            var events2 = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};

            var t1 = eventRepository.AppendAsync(events, 0);
            var t2 = eventRepository2.AppendAsync(events2, 0);

            await Task.WhenAll(t1, t2);

            var res = await eventRepository.AppendAsync(events2, 2);

            Assert.IsTrue(res.Is<Ok>());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task AddAndLoadEventsConcurrent_CacheEmpty(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;
            var eventRepository2 = layerProvider.EventRepository;

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var newGuid2 = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};
            var events2 = new List<IDomainEvent> { new TestEvent1(newGuid2), new TestEvent2(newGuid2)};

            await eventRepository.AppendAsync(events, 0);
            await eventRepository2.AppendAsync(events2, 0);

            var result = await eventRepository.LoadEvents();
            Assert.AreEqual(4, result.Value.Count());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task AddAndLoadEventsConcurrent_CacheEmpty2(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;
            var eventRepository2 = layerProvider.EventRepository;

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};
            var events2 = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};

            await eventRepository.AppendAsync(events, 0);
            await eventRepository2.AppendAsync(events2, 2);

            var result = await eventRepository.LoadEvents();
            Assert.AreEqual(4, result.Value.Count());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task LoadEntityId_NotFoundTIsCorrect(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var entityId = Identity.Create(Guid.NewGuid());
            var result = await eventRepository.LoadEventsByEntity(entityId);

            var notFoundException = Assert.ThrowsException<NotFoundException>(() => result.Value);
            Assert.AreEqual($"Could not find DomainEvents with ID {entityId.Id}", notFoundException.Message);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task LoadEntityId_VersionTooHIgh_NotFoundIsOk(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var entityId = Identity.Create(new Guid());
            var events = new List<IDomainEvent> { new TestEvent1(entityId), new TestEvent2(entityId)};
            await eventRepository.AppendAsync(events, 0);

            var result = await eventRepository.LoadEventsByEntity(entityId, 3);

            Assert.IsTrue(result.Is<Ok>());
            Assert.AreEqual(0, result.Value.Count());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task LoadType_VersionTooHIgh_NotFoundIsOk(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var entityId = Identity.Create(new Guid());
            var events = new List<IDomainEvent> { new TestEvent1(entityId), new TestEvent2(entityId)};
            await eventRepository.AppendAsync(events, 0);

            var result = await eventRepository.LoadEventsByTypeAsync(nameof(TestEvent1), DateTimeOffset.Now.AddDays
            (1));

            Assert.IsTrue(result.Is<Ok>());
            Assert.AreEqual(0, result.Value.Count());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task LoadType_EmptyListWhenNoEventsPresentButAreBeingPublished(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var result = await eventRepository.LoadEventsByTypeAsync("TypeNotInsertedButSubscribed");

            Assert.AreEqual(0, result.Value.Count());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task AddEmptyEventList(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var appendAsync = await eventRepository.AppendAsync(new List<IDomainEvent>(), 0);
            appendAsync.Check();
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task LoadEventsByTypeAsync(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent2(newGuid)};

            await eventRepository.AppendAsync(events, 0);
            var eventsLoaded = await eventRepository.LoadEventsByTypeAsync(typeof(TestEvent2).Name);

            Assert.AreEqual(2, eventsLoaded.Value.Count());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task AddAndLoadEventsByTimeStamp(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent1(newGuid), new TestEvent2(newGuid)};

            await eventRepository.AppendAsync(events, 0);

            var result = await eventRepository.LoadEvents();
            Assert.AreEqual(4, result.Value.Count());
            Assert.AreEqual(1, result.Value.ToList()[0].Version);
            Assert.AreEqual(newGuid.Id, result.Value.ToList()[0].DomainEvent.EntityId.Id);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task AddEvents_FirstEventAfterCreationHasWrongRowVersionBug(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

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

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task AddEvents_VersionTooHigh(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent1(newGuid), new TestEvent2(newGuid)};

            var result = await eventRepository.AppendAsync(events, 1);

            Assert.IsTrue(result.Is<ConcurrencyError>());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task AddEvents_VersionWayTooHigh(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent1(newGuid), new TestEvent2(newGuid)};

            var result = await eventRepository.AppendAsync(events, 5);
            Assert.IsTrue(result.Is<ConcurrencyError>());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task AddAndLoadEventsByTimeStamp_SavedAsType(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var domainEvent = new TestEvent1(newGuid);

            await eventRepository.AppendAsync(new List<IDomainEvent> { domainEvent }, 0);

            var result = await eventRepository.LoadEventsByTypeAsync(typeof(TestEvent1).Name);
            Assert.AreEqual(1, result.Value.Count());
            Assert.AreEqual(1, result.Value.ToList()[0].Version);
            Assert.AreEqual(newGuid.Id, result.Value.ToList()[0].DomainEvent.EntityId.Id);
            Assert.AreEqual(typeof(TestEvent1), result.Value.ToList()[0].DomainEvent.GetType());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task AddEvents_IdSet(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var testEvent1 = new TestEvent1(GuidIdentity.Create(Guid.NewGuid()));
            await eventRepository.AppendAsync(new[] {testEvent1}, 0);

            var result = await eventRepository.LoadEventsByEntity(testEvent1.EntityId);
            var domainEvent = result.Value.Single().DomainEvent;

            Assert.IsTrue(domainEvent.EntityId.Equals(testEvent1.EntityId));
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task AddEvents_IdOfTypeSet(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var testEvent1 = new TestEvent1(GuidIdentity.Create(Guid.NewGuid()));
            await eventRepository.AppendAsync(new List<IDomainEvent> { testEvent1 }, 0);

            var result = await eventRepository.LoadEventsByTypeAsync(testEvent1.GetType().Name);
            var domainEvent = result.Value.Single().DomainEvent;

            Assert.IsTrue(domainEvent.EntityId == testEvent1.EntityId);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task AddEvents_RunTypeProjection(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent1(newGuid), new TestEvent2(newGuid)};

            await eventRepository.AppendAsync(events, 0);

            var result = await eventRepository.LoadEventsByTypeAsync(typeof(TestEvent1).Name);

            Assert.AreEqual(2, result.Value.Count());
            Assert.AreEqual(1, result.Value.ToList()[0].Version);
            Assert.AreEqual(3, result.Value.ToList()[1].Version);
            Assert.IsTrue(newGuid.Equals(result.Value.ToList()[0].DomainEvent.EntityId));
            Assert.AreEqual(typeof(TestEvent1), result.Value.ToList()[0].DomainEvent.GetType());
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task FindLastOccuredOnOfType(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var newGuid = GuidIdentity.Create();
            var events = new List<IDomainEvent> { new TestEvent2(newGuid)};

            await eventRepository.AppendAsync(events, 0);
            var result = await eventRepository.GetLastEventOccuredOn(nameof(TestEvent2));

            await Task.Delay(1000);
            await eventRepository.AppendAsync(events, 1);

            var resultAddedAfter = await eventRepository.GetLastEventOccuredOn(nameof(TestEvent2));

            Assert.AreNotEqual(resultAddedAfter.Value.Ticks, result.Value.Ticks);
            Assert.AreEqual(DateTimeOffset.Now.Day, result.Value.Day);
            Assert.AreEqual(DateTimeOffset.Now.Hour, result.Value.Hour);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task FindLastOccuredOnOfType_NotFound(PersistenceLayerProvider layerProvider)
        {
            var eventRepository = layerProvider.EventRepository;

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent1(newGuid), new TestEvent2(newGuid), new TestEvent2(newGuid)};

            await eventRepository.AppendAsync(events, 0);

            var result = await eventRepository.GetLastEventOccuredOn(nameof(TestEvent3));

            Assert.IsTrue(result.Is<NotFound>());
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