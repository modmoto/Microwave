using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Exceptions;
using Microwave.Domain.Results;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Microwave.EventStores.SnapShots;
using Microwave.Persistence.MongoDb.Eventstores;
using Microwave.Persistence.UnitTestsSetup.MongoDb;
using Moq;

namespace Microwave.Eventstores.UnitTests
{
    [TestClass]
    public class EventStoreTests : IntegrationTests
    {
        [TestMethod]
        public async Task LoadWithNull()
        {
            var eventStore = new EventStore(null, null, new SnapShotConfig());
            var loadAsync = await eventStore.LoadAsync<TestEntity>(null);

            Assert.IsTrue(loadAsync.Is<NotFound>());
            var exception = Assert.ThrowsException<NotFoundException>(() => loadAsync.Value);
            Assert.IsTrue(exception.Message.Contains("null"));
        }

        [TestMethod]
        public async Task ApplyMethod_HappyPath()
        {
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<string>()))
                .ReturnsAsync(SnapShotResult<TestEntity>.Default());

            var entityStremRepo = new Mock<IEventRepository>();
            var entityId = Guid.NewGuid();
            var testEventEventStore = new TestEventEventStore(entityId);
            var domainEventWrapper = new DomainEventWrapper
            {
                DomainEvent  = testEventEventStore
            };
            entityStremRepo.Setup(ev => ev.LoadEventsByEntity(It.IsAny<string>(), It.IsAny<long>()))
                .ReturnsAsync( Result<IEnumerable<DomainEventWrapper>>.Ok( new[] { domainEventWrapper }));
            var eventStore = new EventStore(entityStremRepo.Object, snapShotRepo.Object, new SnapShotConfig());
            var loadAsync = await eventStore.LoadAsync<TestEntity>(entityId.ToString());

            Assert.AreEqual(entityId, loadAsync.Value.Id);
        }

        [TestMethod]
        public async Task ApplyMethod_NoIfDeclared()
        {
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity_NoIApply>(It.IsAny<string>()))
                .ReturnsAsync(SnapShotResult<TestEntity_NoIApply>.Default());

            var entityStremRepo = new Mock<IEventRepository>();
            var entityId = Guid.NewGuid();
            var testEventEventStore = new TestEventEventStore(entityId);
            var domainEventWrapper = new DomainEventWrapper
            {
                DomainEvent  = testEventEventStore
            };
            entityStremRepo.Setup(ev => ev.LoadEventsByEntity(It.IsAny<string>(), It.IsAny<long>()))
                .ReturnsAsync( Result<IEnumerable<DomainEventWrapper>>.Ok( new[] { domainEventWrapper }));
            var eventStore = new EventStore(entityStremRepo.Object, snapShotRepo.Object, new SnapShotConfig());
            var loadAsync = await eventStore.LoadAsync<TestEntity_NoIApply>(entityId.ToString());

            Assert.AreEqual(Guid.Empty, loadAsync.Value.Id);
        }

        [TestMethod]
        public async Task ApplyMethod_WrongIfDeclared()
        {
            var entityStremRepo = new Mock<IEventRepository>();
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity_NoIApply>(It.IsAny<string>()))
                .ReturnsAsync(SnapShotResult<TestEntity_NoIApply>.Default());
            var entityId = Guid.NewGuid();
            var testEventEventStore = new TestEventEventStore(entityId);
            var domainEventWrapper = new DomainEventWrapper
            {
                DomainEvent  = testEventEventStore
            };
            entityStremRepo.Setup(ev => ev.LoadEventsByEntity(It.IsAny<string>(), It.IsAny<long>()))
                .ReturnsAsync( Result<IEnumerable<DomainEventWrapper>>.Ok( new[] { domainEventWrapper }));
            var eventStore = new EventStore(entityStremRepo.Object, snapShotRepo.Object, new SnapShotConfig());
            var loadAsync = await eventStore.LoadAsync<TestEntity_NoIApply>(entityId.ToString());

            Assert.AreEqual(Guid.Empty, loadAsync.Value.Id);
        }

        [TestMethod]
        public async Task IntegrationWithRepo()
        {
            BsonMapRegistrationHelpers.AddBsonMapFor<TestEventEventStore>();
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<string>()))
                .ReturnsAsync(SnapShotResult<TestEntity>.Default());
            var entityId = Guid.NewGuid();
            var eventStore = new EventStore(
                new EventRepositoryMongoDb(EventMongoDb,
                new VersionCache(EventMongoDb)),
                snapShotRepo.Object, new SnapShotConfig());

            await eventStore.AppendAsync(new List<IDomainEvent> {new TestEventEventStore(entityId, "Test")}, 0);
            var loadAsync = await eventStore.LoadAsync<TestEntity>(entityId.ToString());
            var loadAsync2 = await eventStore.LoadAsync<TestEntity>(entityId.ToString());

            Assert.AreEqual(entityId, loadAsync.Value.Id);
            Assert.AreEqual("Test", loadAsync.Value.Name);

            Assert.AreEqual(entityId, loadAsync2.Value.Id);
            Assert.AreEqual("Test", loadAsync2.Value.Name);
        }

        [TestMethod]
        public async Task IntegrationWithRepo_AddSingleEvent()
        {
            BsonMapRegistrationHelpers.AddBsonMapFor<TestEventEventStore>();
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<string>()))
                .ReturnsAsync(SnapShotResult<TestEntity>.Default());
            var entityId = Guid.NewGuid();
            var eventStore = new EventStore(new EventRepositoryMongoDb(EventMongoDb, new VersionCache(EventMongoDb)), snapShotRepo.Object, new SnapShotConfig());

            await eventStore.AppendAsync(new TestEventEventStore(entityId, "Test"), 0);
            var loadAsync = await eventStore.LoadAsync<TestEntity>(entityId.ToString());

            Assert.AreEqual(entityId, loadAsync.Value.Id);
            Assert.AreEqual("Test", loadAsync.Value.Name);
        }

        [TestMethod]
        public async Task DifferentIdsInEventsDefined()
        {
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<string>()))
                .ReturnsAsync(SnapShotResult<TestEntity>.Default());
            var entityId = Guid.NewGuid();
            var entityId2 = Guid.NewGuid();
            var eventStore = new EventStore(new EventRepositoryMongoDb(EventMongoDb, new VersionCache(EventMongoDb)), snapShotRepo.Object, new SnapShotConfig());

            await Assert.ThrowsExceptionAsync<DifferentIdsException>(async () => await eventStore.AppendAsync(new
            List<IDomainEvent> {new
            TestEventEventStore(entityId, "Test"), new
                TestEventEventStore(entityId2, "Test")}, 0));
        }

        [TestMethod]
        public async Task NotFoundExceptionIsWithCorrectT()
        {
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<string>()))
                .ReturnsAsync(SnapShotResult<TestEntity>.Default());
            var entityId = Guid.NewGuid();
            var eventStore = new EventStore(new EventRepositoryMongoDb(EventMongoDb, new VersionCache(EventMongoDb)), snapShotRepo.Object, new SnapShotConfig());

            var result = await eventStore.LoadAsync<TestEntity>(entityId.ToString());
            var exception = Assert.ThrowsException<NotFoundException>(() => result.Value);

            Assert.IsTrue(exception.Message.StartsWith("Could not find TestEntity"));

        }

        [TestMethod]
        public async Task IntegrationWithRepo_NotFound()
        {
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<string>()))
                .ReturnsAsync(SnapShotResult<TestEntity>.Default());
            var entityId = Guid.NewGuid();
            var eventStore = new EventStore(new EventRepositoryMongoDb(EventMongoDb, new VersionCache(EventMongoDb)), snapShotRepo.Object, new SnapShotConfig());

            await eventStore.AppendAsync(new List<IDomainEvent> {new TestEventEventStore(entityId, "Test")}, 0);
            var loadAsync = await eventStore.LoadAsync<TestEntity>(Guid.NewGuid().ToString());

            Assert.IsTrue(loadAsync.Is<NotFound>());
        }
    }

    public class TestEntity_WrongIApply : Entity
    {
        public string Id { get; private set; }
        public void Apply(WrongEvent domainEvent)
        {
            Id = domainEvent.EntityId;
        }
    }

    public class WrongEvent : IDomainEvent
    {
        public WrongEvent(string entityId)
        {
            EntityId = entityId;
        }

        public string EntityId { get; }
    }

    public class TestEntity_NoIApply : Entity
    {
        public Guid Id { get; private set; }
    }

    public class TestEntity : Entity, IApply<TestEventEventStore>
    {
        public void Apply(TestEventEventStore domainEvent)
        {
            Id = domainEvent.Id;
            Name = domainEvent.Name;
        }

        public Guid Id { get; private set; }
        public string Name { get; set; }
    }

    public class TestEventEventStore : IDomainEvent
    {
        public TestEventEventStore(Guid id, string name = null)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; }
        public string Name { get; }
        public string EntityId => Id.ToString();
    }
}