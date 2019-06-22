using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.EventStores;
using Microwave.Eventstores.Persistence.MongoDb;
using Microwave.EventStores.Ports;
using Microwave.Persistence.MongoDb;
using Microwave.Persistence.MongoDb.UnitTests.Eventstores;
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

            var eventRepository = new EventRepository(EventDatabase, new VersionCache(EventDatabase));
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<Identity>()))
                .ReturnsAsync(new DefaultSnapshot<TestEntity>());

            var eventStore = new EventStore(eventRepository, snapShotRepo.Object);

            var newGuid = Guid.NewGuid();

            await eventStore.AppendAsync(new List<IDomainEvent> { new TestEvent_BsonBug(GuidIdentity.Create(newGuid), "Simon")},
                0);

            var result = await eventRepository.LoadEvents();

            Assert.AreEqual(1, result.Value.Count());
            Assert.AreEqual(newGuid.ToString(), result.Value.Single().DomainEvent.EntityId.Id);
            Assert.AreEqual("Simon", ((TestEvent_BsonBug)result.Value.Single().DomainEvent).Name);
        }

        [TestMethod]
        public async Task AddEvents_OneAutoProp()
        {
            BsonMapRegistrationHelpers.AddBsonMapFor<TestEvent_BsonBug_AutoProperty>();

            var eventRepository = new EventRepository(EventDatabase, new VersionCache(EventDatabase));
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<Identity>()))
                .ReturnsAsync(new DefaultSnapshot<TestEntity>());

            var eventStore = new EventStore(eventRepository, snapShotRepo.Object);

            var newGuid = Guid.NewGuid();

            await eventStore.AppendAsync(new List<IDomainEvent> { new TestEvent_BsonBug_AutoProperty(GuidIdentity.Create(newGuid), "Simon")},
                0);

            var result = await eventRepository.LoadEvents();

            Assert.AreEqual(1, result.Value.Count());
            Assert.AreEqual(newGuid.ToString(), result.Value.Single().DomainEvent.EntityId.Id);
            Assert.AreEqual("Simon", ((TestEvent_BsonBug_AutoProperty)result.Value.Single().DomainEvent).Name);
            Assert.AreEqual(newGuid.ToString(), ((TestEvent_BsonBug_AutoProperty)result.Value.Single().DomainEvent).EntityIdAsString);
        }

        [TestMethod]
        public async Task AddEvents_ConstructorBson_UnconventionalOderring()
        {
            BsonMapRegistrationHelpers.AddBsonMapFor<TestEvent_UnconventionalOderring>();

            var eventRepository = new EventRepository(EventDatabase, new VersionCache(EventDatabase));
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<Identity>()))
                .ReturnsAsync(new DefaultSnapshot<TestEntity>());

            var eventStore = new EventStore(eventRepository, snapShotRepo.Object);

            await eventStore.AppendAsync(new List<IDomainEvent> { new TestEvent_UnconventionalOderring("Simon", StringIdentity.Create("whatever"))},
                0);

            var result = await eventRepository.LoadEvents();

            Assert.AreEqual(1, result.Value.Count());
            Assert.AreEqual("whatever", result.Value.Single().DomainEvent.EntityId.Id);
            Assert.AreEqual("Simon", ((TestEvent_UnconventionalOderring)result.Value.Single().DomainEvent).Name);
        }

        [TestMethod]
        public async Task AddEvents_ConstructorBson_TwoIdentitiesInConstructor()
        {
            BsonMapRegistrationHelpers.AddBsonMapFor<TestEvent_TwoIdentitiesInConstructor>();

            var eventRepository = new EventRepository(EventDatabase, new VersionCache(EventDatabase));
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<Identity>()))
                .ReturnsAsync(new DefaultSnapshot<TestEntity>());

            var eventStore = new EventStore(eventRepository, snapShotRepo.Object);

            var newGuid = Guid.NewGuid();
            await eventStore.AppendAsync(new List<IDomainEvent> { new TestEvent_TwoIdentitiesInConstructor(StringIdentity
            .Create("whatever"), GuidIdentity.Create(newGuid))},
                0);

            var result = await eventRepository.LoadEvents();

            Assert.AreEqual(1, result.Value.Count());
            Assert.AreEqual("whatever", result.Value.Single().DomainEvent.EntityId.Id);
            Assert.AreEqual(newGuid.ToString(), ((TestEvent_TwoIdentitiesInConstructor)result.Value.Single().DomainEvent).GuidIdentity
            .Id);
        }

        [TestMethod]
        public async Task AddEvents_AutoProperty_NotNamedEntityId()
        {
            BsonMapRegistrationHelpers.AddBsonMapFor<TestEvent_BsonBug_AutoProperty_NotWithEntityIdName>();

            var eventRepository = new EventRepository(EventDatabase, new VersionCache(EventDatabase));
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<Identity>()))
                .ReturnsAsync(new DefaultSnapshot<TestEntity>());

            var eventStore = new EventStore(eventRepository, snapShotRepo.Object);

            await eventStore.AppendAsync(new List<IDomainEvent> { new TestEvent_BsonBug_AutoProperty_NotWithEntityIdName(StringIdentity
                    .Create("whatever"), "Peter")},
                0);

            var result = await eventRepository.LoadEvents();

            Assert.AreEqual(1, result.Value.Count());
            var domainEvent = result.Value.Single().DomainEvent as TestEvent_BsonBug_AutoProperty_NotWithEntityIdName;
            Assert.AreEqual("whatever", domainEvent.EntityId.Id);
            Assert.AreEqual("Peter", domainEvent.Name);
        }
    }

    public class TestEvent_NotEntityIdDefined : IDomainEvent
    {
        public TestEvent_NotEntityIdDefined(StringIdentity create, string name)
        {
            EntityId = create;
            Name = name;
        }

        public Identity EntityId { get; }
        public string Name { get; }
    }

    public class TestEvent_ParamDefinedWrong : IDomainEvent
    {
        public TestEvent_ParamDefinedWrong(StringIdentity entityId, string name_NOT_DEFINED_RIGHT)
        {
            EntityId = entityId;
            Name = name_NOT_DEFINED_RIGHT;
        }

        public Identity EntityId { get; }
        public string Name { get; }
    }

    public class TestEvent_TwoIdentitiesInConstructor : IDomainEvent
    {
        public TestEvent_TwoIdentitiesInConstructor(StringIdentity entityId, GuidIdentity guidIdentity)
        {
            EntityId = entityId;
            GuidIdentity = guidIdentity;
        }

        public Identity EntityId { get; }
        public GuidIdentity GuidIdentity { get; }
    }

    public class TestEvent_UnconventionalOderring : IDomainEvent
    {
        public Identity EntityId { get; }
        public string Name { get; }

        public TestEvent_UnconventionalOderring(string name, StringIdentity entityId)
        {
            EntityId = entityId;
            Name = name;
        }
    }

    public class TestEvent_BsonBug : IDomainEvent
    {
        public string Name { get; }

        public TestEvent_BsonBug(GuidIdentity entityId, string name)
        {
            Name = name;
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }

    public class TestEvent_BsonBug_AutoProperty : IDomainEvent
    {
        public string Name { get; }

        public TestEvent_BsonBug_AutoProperty(GuidIdentity entityId, string name)
        {
            Name = name;
            EntityId = entityId;
        }

        public Identity EntityId { get; }
        public string EntityIdAsString => EntityId.Id;
    }

    public class TestEvent_BsonBug_AutoProperty_NotWithEntityIdName : IDomainEvent
    {
        public StringIdentity AutoPropertyId { get; }
        public string Name { get; }

        public TestEvent_BsonBug_AutoProperty_NotWithEntityIdName(StringIdentity autoPropertyId, string name)
        {
            AutoPropertyId = autoPropertyId;
            Name = name;
        }

        public Identity EntityId => AutoPropertyId;
    }
}