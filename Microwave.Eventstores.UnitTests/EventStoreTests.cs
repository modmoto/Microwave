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
    public class EventStoreTests
    {
        [TestMethod]
        public async Task ApplyMethod_HappyPath()
        {
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<Guid>()))
                .ReturnsAsync(new DefaultSnapshot<TestEntity>());

            var entityStremRepo = new Mock<IEventRepository>();
            var entityId = Guid.NewGuid();
            var testEventEventStore = new TestEventEventStore(entityId);
            var domainEventWrapper = new DomainEventWrapper
            {
                DomainEvent  = testEventEventStore
            };
            entityStremRepo.Setup(ev => ev.LoadEventsByEntity(It.IsAny<Guid>(), It.IsAny<long>()))
                .ReturnsAsync( Result<IEnumerable<DomainEventWrapper>>.Ok( new[] { domainEventWrapper }));
            var snapShotMock = new Mock<ISnapShotConfig>();
            var eventStore = new EventStore(entityStremRepo.Object, snapShotRepo.Object, snapShotMock.Object);
            var loadAsync = await eventStore.LoadAsync<TestEntity>(entityId);

            Assert.AreEqual(entityId, loadAsync.Entity.Id);
        }

        [TestMethod]
        public async Task ApplyMethod_NoIfDeclared()
        {
            var snapShotMock = new Mock<ISnapShotConfig>();
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity_NoIApply>(It.IsAny<Guid>()))
                .ReturnsAsync(new DefaultSnapshot<TestEntity_NoIApply>());

            var entityStremRepo = new Mock<IEventRepository>();
            var entityId = Guid.NewGuid();
            var testEventEventStore = new TestEventEventStore(entityId);
            var domainEventWrapper = new DomainEventWrapper
            {
                DomainEvent  = testEventEventStore
            };
            entityStremRepo.Setup(ev => ev.LoadEventsByEntity(It.IsAny<Guid>(), It.IsAny<long>()))
                .ReturnsAsync( Result<IEnumerable<DomainEventWrapper>>.Ok( new[] { domainEventWrapper }));
            var eventStore = new EventStore(entityStremRepo.Object, snapShotRepo.Object, snapShotMock.Object);
            var loadAsync = await eventStore.LoadAsync<TestEntity_NoIApply>(entityId);

            Assert.AreEqual(Guid.Empty, loadAsync.Entity.Id);
        }

        [TestMethod]
        public async Task ApplyMethod_WrongIfDeclared()
        {
            var entityStremRepo = new Mock<IEventRepository>();
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity_NoIApply>(It.IsAny<Guid>()))
                .ReturnsAsync(new DefaultSnapshot<TestEntity_NoIApply>());
            var entityId = Guid.NewGuid();
            var testEventEventStore = new TestEventEventStore(entityId);
            var domainEventWrapper = new DomainEventWrapper
            {
                DomainEvent  = testEventEventStore
            };
            entityStremRepo.Setup(ev => ev.LoadEventsByEntity(It.IsAny<Guid>(), It.IsAny<long>()))
                .ReturnsAsync( Result<IEnumerable<DomainEventWrapper>>.Ok( new[] { domainEventWrapper }));
            var snapShotMock = new Mock<ISnapShotConfig>();
            var eventStore = new EventStore(entityStremRepo.Object, snapShotRepo.Object, snapShotMock.Object);
            var loadAsync = await eventStore.LoadAsync<TestEntity_NoIApply>(entityId);

            Assert.AreEqual(Guid.Empty, loadAsync.Entity.Id);
        }
    }

    public class TestEntity_WrongIApply : Entity
    {
        public Guid Id { get; private set; }
        public void Apply(WrongEvent domainEvent)
        {
            Id = domainEvent.EntityId;
        }
    }

    public class WrongEvent : IDomainEvent
    {
        public WrongEvent(Guid entityId)
        {
            EntityId = entityId;
        }

        public Guid EntityId { get; }
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
        }

        public Guid Id { get; private set; }
    }

    public class TestEventEventStore : IDomainEvent
    {
        public TestEventEventStore(Guid entityId)
        {
            EntityId = entityId;
        }

        public Guid EntityId { get; }
    }
}