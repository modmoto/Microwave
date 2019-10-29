using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Microwave.Persistence.MongoDb.Eventstores;
using Microwave.Persistence.UnitTestsSetup.MongoDb;
using Moq;

namespace Microwave.Eventstores.UnitTests
{
    [TestClass]
    public class BsonMapRegistrationHelperTests : IntegrationTests
    {
        [TestMethod]
        public async Task AddEvents_ConstructorBsonBug()
        {
            BsonMapRegistrationHelpers.AddBsonMapFor<TestEvent_BsonBug>();

            var eventRepository = new EventRepositoryMongoDb(EventMongoDb, new VersionCache(EventMongoDb));
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<string>()))
                .ReturnsAsync(SnapShotResult<TestEntity>.Default());

            var eventStore = new EventStore(eventRepository, snapShotRepo.Object);

            var newGuid = Guid.NewGuid();

            await eventStore.AppendAsync(new List<IDomainEvent> { new TestEvent_BsonBug(newGuid, "Simon")},
                0);

            var result = await eventRepository.LoadEvents();

            Assert.AreEqual(1, result.Value.Count());
            Assert.AreEqual(newGuid.ToString(), result.Value.Single().DomainEvent.EntityId);
            Assert.AreEqual("Simon", ((TestEvent_BsonBug)result.Value.Single().DomainEvent).Name);
        }

        [TestMethod]
        public async Task AddEvents_OneAutoProp()
        {
            BsonMapRegistrationHelpers.AddBsonMapFor<TestEvent_BsonBug_AutoProperty>();

            var eventRepository = new EventRepositoryMongoDb(EventMongoDb, new VersionCache(EventMongoDb));
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<string>()))
                .ReturnsAsync(SnapShotResult<TestEntity>.Default());

            var eventStore = new EventStore(eventRepository, snapShotRepo.Object);

            var newGuid = Guid.NewGuid();

            await eventStore.AppendAsync(new List<IDomainEvent> { new TestEvent_BsonBug_AutoProperty(newGuid.ToString(),
            "Simon")},
                0);

            var result = await eventRepository.LoadEvents();

            Assert.AreEqual(1, result.Value.Count());
            Assert.AreEqual(newGuid.ToString(), result.Value.Single().DomainEvent.EntityId);
            Assert.AreEqual("Simon", ((TestEvent_BsonBug_AutoProperty)result.Value.Single().DomainEvent).Name);
            Assert.AreEqual(newGuid.ToString(), ((TestEvent_BsonBug_AutoProperty)result.Value.Single().DomainEvent).EntityIdAsString);
        }

        [TestMethod]
        public async Task AddEvents_ConstructorBson_UnconventionalOderring()
        {
            BsonMapRegistrationHelpers.AddBsonMapFor<TestEvent_UnconventionalOderring>();

            var eventRepository = new EventRepositoryMongoDb(EventMongoDb, new VersionCache(EventMongoDb));
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<string>()))
                .ReturnsAsync(SnapShotResult<TestEntity>.Default());

            var eventStore = new EventStore(eventRepository, snapShotRepo.Object);

            await eventStore.AppendAsync(new List<IDomainEvent> { new TestEvent_UnconventionalOderring("Simon", "whatever")},
                0);

            var result = await eventRepository.LoadEvents();

            Assert.AreEqual(1, result.Value.Count());
            Assert.AreEqual("whatever", result.Value.Single().DomainEvent.EntityId);
            Assert.AreEqual("Simon", ((TestEvent_UnconventionalOderring)result.Value.Single().DomainEvent).Name);
        }

        [TestMethod]
        public async Task AddEvents_AutoProperty_NotNamedEntityId()
        {
            BsonMapRegistrationHelpers.AddBsonMapFor<TestEvent_BsonBug_AutoProperty_NotWithEntityIdName>();

            var eventRepository = new EventRepositoryMongoDb(EventMongoDb, new VersionCache(EventMongoDb));
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<string>()))
                .ReturnsAsync(SnapShotResult<TestEntity>.Default());

            var eventStore = new EventStore(eventRepository, snapShotRepo.Object);

            await eventStore.AppendAsync(new List<IDomainEvent> {
                    new TestEvent_BsonBug_AutoProperty_NotWithEntityIdName("whatever", "Peter")},
                0);

            var result = await eventRepository.LoadEvents();

            Assert.AreEqual(1, result.Value.Count());
            var domainEvent = result.Value.Single().DomainEvent as TestEvent_BsonBug_AutoProperty_NotWithEntityIdName;
            Assert.AreEqual("whatever", domainEvent.EntityId);
            Assert.AreEqual("Peter", domainEvent.Name);
        }
    }

    public class TestEvent_NotEntityIdDefined : IDomainEvent
    {
        public TestEvent_NotEntityIdDefined(Guid create, string name)
        {
            EntityId = create.ToString();
            Name = name;
        }

        public string EntityId { get; }
        public string Name { get; }
    }

    public class TestEvent_ParamDefinedWrong : IDomainEvent
    {
        public TestEvent_ParamDefinedWrong(Guid entityId, string name_NOT_DEFINED_RIGHT)
        {
            EntityId = entityId.ToString();
            Name = name_NOT_DEFINED_RIGHT;
        }

        public string EntityId { get; }
        public string Name { get; }
    }

    public class TestEvent_UnconventionalOderring : IDomainEvent
    {
        public string EntityId { get; }
        public string Name { get; }

        public TestEvent_UnconventionalOderring(string name, string entityId)
        {
            EntityId = entityId;
            Name = name;
        }
    }

    public class TestEvent_BsonBug : IDomainEvent
    {
        public string Name { get; }

        public TestEvent_BsonBug(Guid id, string name)
        {
            Name = name;
            Id = id;
        }

        public string EntityId => Id.ToString();
        public Guid Id { get; }
    }

    public class TestEvent_BsonBug_AutoProperty : IDomainEvent
    {
        public string Name { get; }

        public TestEvent_BsonBug_AutoProperty(string entityId, string name)
        {
            Name = name;
            EntityId = entityId;
        }

        public string EntityId { get; }
        public string EntityIdAsString => EntityId;
    }

    public class TestEvent_BsonBug_AutoProperty_NotWithEntityIdName : IDomainEvent
    {
        public string AutoPropertyId { get; }
        public string Name { get; }

        public TestEvent_BsonBug_AutoProperty_NotWithEntityIdName(string autoPropertyId, string name)
        {
            AutoPropertyId = autoPropertyId;
            Name = name;
        }

        public string EntityId => AutoPropertyId;
    }
}