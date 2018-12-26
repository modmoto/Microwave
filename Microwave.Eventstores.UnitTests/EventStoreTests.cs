using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application;
using Microwave.Application.Results;
using Microwave.Domain;
using Microwave.EventStores;
using Microwave.Queries;
using Moq;

namespace Microwave.Eventstores.UnitTests
{
    [TestClass]
    public class EventStoreTests : IntegrationTests
    {
        [TestMethod]
        public async Task ApplyMethod_HappyPath()
        {
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<Identity>()))
                .ReturnsAsync(new DefaultSnapshot<TestEntity>());

            var entityStremRepo = new Mock<IEventRepository>();
            var entityId = GuidIdentity.Create(Guid.NewGuid());
            var testEventEventStore = new TestEventEventStore(entityId);
            var domainEventWrapper = new DomainEventWrapper
            {
                DomainEvent  = testEventEventStore
            };
            entityStremRepo.Setup(ev => ev.LoadEventsByEntity(It.IsAny<Identity>(), It.IsAny<long>()))
                .ReturnsAsync( Result<IEnumerable<DomainEventWrapper>>.Ok( new[] { domainEventWrapper }));
            var eventStore = new EventStore(entityStremRepo.Object, snapShotRepo.Object);
            var loadAsync = await eventStore.LoadAsync<TestEntity>(entityId);

            Assert.AreEqual(entityId, loadAsync.Entity.Id);
        }

        [TestMethod]
        public async Task ApplyMethod_NoIfDeclared()
        {
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity_NoIApply>(It.IsAny<Identity>()))
                .ReturnsAsync(new DefaultSnapshot<TestEntity_NoIApply>());

            var entityStremRepo = new Mock<IEventRepository>();
            var entityId = GuidIdentity.Create(Guid.NewGuid());
            var testEventEventStore = new TestEventEventStore(entityId);
            var domainEventWrapper = new DomainEventWrapper
            {
                DomainEvent  = testEventEventStore
            };
            entityStremRepo.Setup(ev => ev.LoadEventsByEntity(It.IsAny<Identity>(), It.IsAny<long>()))
                .ReturnsAsync( Result<IEnumerable<DomainEventWrapper>>.Ok( new[] { domainEventWrapper }));
            var eventStore = new EventStore(entityStremRepo.Object, snapShotRepo.Object);
            var loadAsync = await eventStore.LoadAsync<TestEntity_NoIApply>(entityId);

            Assert.AreEqual(Guid.Empty, loadAsync.Entity.Id);
        }

        [TestMethod]
        public async Task ApplyMethod_WrongIfDeclared()
        {
            var entityStremRepo = new Mock<IEventRepository>();
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity_NoIApply>(It.IsAny<Identity>()))
                .ReturnsAsync(new DefaultSnapshot<TestEntity_NoIApply>());
            var entityId = GuidIdentity.Create(Guid.NewGuid());
            var testEventEventStore = new TestEventEventStore(entityId);
            var domainEventWrapper = new DomainEventWrapper
            {
                DomainEvent  = testEventEventStore
            };
            entityStremRepo.Setup(ev => ev.LoadEventsByEntity(It.IsAny<Identity>(), It.IsAny<long>()))
                .ReturnsAsync( Result<IEnumerable<DomainEventWrapper>>.Ok( new[] { domainEventWrapper }));
            var eventStore = new EventStore(entityStremRepo.Object, snapShotRepo.Object);
            var loadAsync = await eventStore.LoadAsync<TestEntity_NoIApply>(entityId);

            Assert.AreEqual(Guid.Empty, loadAsync.Entity.Id);
        }

        [TestMethod]
        public async Task IntegrationWithRepo()
        {
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<Identity>()))
                .ReturnsAsync(new DefaultSnapshot<TestEntity>());
            var entityId = GuidIdentity.Create(Guid.NewGuid());
            var eventStore = new EventStore(new EventRepository(new EventDatabase(Database)), snapShotRepo.Object);

            await eventStore.AppendAsync(new List<IDomainEvent> {new TestEventEventStore(entityId, "Test")}, 0);
            var loadAsync = await eventStore.LoadAsync<TestEntity>(entityId);
            var loadAsync2 = await eventStore.LoadAsync<TestEntity>(entityId);

            Assert.IsTrue(entityId.Equals(loadAsync.Entity.Id));
            Assert.AreEqual("Test", loadAsync.Entity.Name);

            Assert.IsTrue(entityId.Equals(loadAsync2.Entity.Id));
            Assert.AreEqual("Test", loadAsync2.Entity.Name);
        }
    }

    public class TestEntity_WrongIApply : Entity
    {
        public Identity Id { get; private set; }
        public void Apply(WrongEvent domainEvent)
        {
            Id = domainEvent.EntityId;
        }
    }

    public class WrongEvent : IDomainEvent
    {
        public WrongEvent(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }

    public class TestEntity_NoIApply : Entity
    {
        public Guid Id { get; private set; }
    }

    public class TestEntity : Entity
    {
        public void Apply(TestEventEventStore domainEvent)
        {
            Id = domainEvent.EntityId;
            Name = domainEvent.Name;
        }

        public Identity Id { get; private set; }
        public string Name { get; set; }
    }

    public class TestEventEventStore : IDomainEvent
    {
        public TestEventEventStore(Identity entityId, string name = null)
        {
            EntityId = entityId;
            Name = name;
        }

        public Identity EntityId { get; }
        public string Name { get; }
    }
}