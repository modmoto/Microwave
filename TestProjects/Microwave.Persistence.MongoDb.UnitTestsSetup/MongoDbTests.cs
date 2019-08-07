using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Microwave.Persistence.MongoDb.Eventstores;
using MongoDB.Bson.Serialization;
using Moq;

namespace Microwave.Persistence.MongoDb.UnitTestsSetup
{
    [TestClass]
    public class MongoDbTests : IntegrationTests
    {
        [TestMethod]
        public async Task AddAndLoadEvents_AutoProperty()
        {
            var eventRepository = new EventRepositoryMongoDb(EventMongoDb, new VersionCache(EventMongoDb));
            var stringIdentity = StringIdentity.Create("TestId");

            BsonClassMap.RegisterClassMap<DomainEventWithAutoProperty>(cm =>
            {
                cm.MapCreator(ev => new DomainEventWithAutoProperty(ev.CreateId, ev.TestProperty));
                cm.MapProperty(nameof(DomainEventWithAutoProperty.CreateId));
                cm.MapProperty(nameof(DomainEventWithAutoProperty.TestProperty));
            });

            await eventRepository.AppendAsync(new List<IDomainEvent>
            {
                new DomainEventWithAutoProperty(stringIdentity, "TestProperty")
            }, 0);

            var loadEventsByEntity = await eventRepository.LoadEventsByEntity(stringIdentity);

            var loadedEvent = loadEventsByEntity.Value.Single().DomainEvent as DomainEventWithAutoProperty;
            Assert.AreEqual(loadedEvent.CreateId, stringIdentity);
            Assert.AreEqual(loadedEvent.EntityId, stringIdentity);
            Assert.AreEqual(loadedEvent.TestProperty, "TestProperty");
        }

        [TestMethod]
        public async Task AddEvents_ForeachMethod()
        {
            BsonMapRegistrationHelpers.AddBsonMapFor<TestEventAllOk>();

            var eventRepository = new EventRepositoryMongoDb(EventMongoDb, new VersionCache(EventMongoDb));
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<Identity>()))
                .ReturnsAsync(SnapShotResult<TestEntity>.Default());

            var eventStore = new EventStore(eventRepository, snapShotRepo.Object);

            var newGuid = Guid.NewGuid();

            await eventStore.AppendAsync(
                new List<IDomainEvent> {new TestEventAllOk(GuidIdentity.Create(newGuid), "Simon")},
                0);

            var result = await eventRepository.LoadEvents();

            Assert.AreEqual(1, result.Value.Count());
            Assert.AreEqual(newGuid.ToString(), result.Value.Single().DomainEvent.EntityId.Id);
            Assert.AreEqual("Simon", ((TestEventAllOk) result.Value.Single().DomainEvent).Name);
        }

        // this is because of mongodb, constructor has to be named the same
        [TestMethod]
        public async Task AddAndLoadEvents_ParamCalledWrong()
        {
            var eventRepository = new EventRepositoryMongoDb(EventMongoDb, new VersionCache(EventMongoDb));

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent_ParameterCalledWrong(newGuid)};
            await eventRepository.AppendAsync(events, 0);

            var loadEventsByEntity = await eventRepository.LoadEventsByEntity(newGuid);
            Assert.AreEqual(1, loadEventsByEntity.Value.Count());
            Assert.AreEqual(null, loadEventsByEntity.Value.ToList()[0].DomainEvent.EntityId);
        }

        [TestMethod]
        public async Task AddAndLoadEventsConcurrent_CacheEmpty2()
        {
            var eventRepository = new EventRepositoryMongoDb(EventMongoDb, new VersionCache(EventMongoDb));
            var eventRepository2 = new EventRepositoryMongoDb(EventMongoDb, new VersionCache(EventMongoDb));

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};
            var events2 = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};

            await eventRepository.AppendAsync(events, 0);
            await eventRepository2.AppendAsync(events2, 2);

            var result = await eventRepository.LoadEvents();
            Assert.AreEqual(4, result.Value.Count());
        }

        [TestMethod]
        public async Task AddAndLoadEventsConcurrent_CacheEmpty()
        {
            var eventRepository = new EventRepositoryMongoDb(EventMongoDb, new VersionCache(EventMongoDb));;
            var eventRepository2 = new EventRepositoryMongoDb(EventMongoDb, new VersionCache(EventMongoDb));

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var newGuid2 = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};
            var events2 = new List<IDomainEvent> { new TestEvent1(newGuid2), new TestEvent2(newGuid2)};

            await eventRepository.AppendAsync(events, 0);
            await eventRepository2.AppendAsync(events2, 0);

            var result = await eventRepository.LoadEvents();
            Assert.AreEqual(4, result.Value.Count());
        }

        [TestMethod]
        public async Task LoadEntityId_VersionTooHIgh_NotFoundIsOk()
        {
            var eventRepository = new EventRepositoryMongoDb(EventMongoDb, new VersionCache(EventMongoDb));

            var newGuid = GuidIdentity.Create(Guid.NewGuid());
            var events = new List<IDomainEvent> { new TestEvent1(newGuid), new TestEvent2(newGuid)};
            await eventRepository.AppendAsync(events, 0);

            var result = await eventRepository.LoadEventsByEntity(newGuid, 3);

            Assert.IsTrue(result.Is<Ok>());
            Assert.AreEqual(0, result.Value.Count());
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

    public class TestEvent2 : IDomainEvent
    {
        public TestEvent2(Identity entityId)
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

    public class TestEntity
    {
    }

    public class TestEventAllOk : IDomainEvent
    {
        public TestEventAllOk(GuidIdentity entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }

        public Identity EntityId { get; }
        public string Name { get; }
    }

    public class DomainEventWithAutoProperty : IDomainEvent
    {
        public StringIdentity CreateId { get; }
        public string TestProperty { get; }

        public Identity EntityId => CreateId;

        public DomainEventWithAutoProperty(StringIdentity createId, string testProperty)
        {
            CreateId = createId;
            TestProperty = testProperty;
        }

    }
}