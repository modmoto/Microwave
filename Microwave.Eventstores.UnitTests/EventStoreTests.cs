using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application;
using Microwave.Application.Ports;
using Microwave.Application.Results;
using Microwave.Domain;
using Microwave.EventStores;
using Moq;

namespace Microwave.Eventstores.UnitTests
{
    [TestClass]
    public class EventStoreTests
    {
        [TestMethod]
        public async Task ApplyMethod_HappyPath()
        {
            var entityStremRepo = new Mock<IEntityStreamRepository>();
            var entityId = Guid.NewGuid();
            var testEventEventStore = new TestEventEventStore(entityId);
            var domainEventWrapper = new DomainEventWrapper
            {
                DomainEvent  = testEventEventStore
            };
            entityStremRepo.Setup(ev => ev.LoadEventsByEntity(It.IsAny<Guid>(), It.IsAny<long>()))
                .ReturnsAsync( Result<IEnumerable<DomainEventWrapper>>.Ok( new[] { domainEventWrapper }));
            var eventStore = new EventStore(entityStremRepo.Object, new EventSourcingApplyStrategy());
            var loadAsync = await eventStore.LoadAsync<TestEntity>(entityId);

            Assert.AreEqual(entityId, loadAsync.Entity.Id);
        }

        [TestMethod]
        public async Task ApplyMethod_NoIfDeclared()
        {
            var entityStremRepo = new Mock<IEntityStreamRepository>();
            var entityId = Guid.NewGuid();
            var testEventEventStore = new TestEventEventStore(entityId);
            var domainEventWrapper = new DomainEventWrapper
            {
                DomainEvent  = testEventEventStore
            };
            entityStremRepo.Setup(ev => ev.LoadEventsByEntity(It.IsAny<Guid>(), It.IsAny<long>()))
                .ReturnsAsync( Result<IEnumerable<DomainEventWrapper>>.Ok( new[] { domainEventWrapper }));
            var eventStore = new EventStore(entityStremRepo.Object, new EventSourcingApplyStrategy());
            var loadAsync = await eventStore.LoadAsync<TestEntity_NoIApply>(entityId);

            Assert.AreEqual(new Guid(), loadAsync.Entity.Id);
        }

        [TestMethod]
        public async Task ApplyMethod_WrongIfDeclared()
        {
            var entityStremRepo = new Mock<IEntityStreamRepository>();
            var entityId = Guid.NewGuid();
            var testEventEventStore = new TestEventEventStore(entityId);
            var domainEventWrapper = new DomainEventWrapper
            {
                DomainEvent  = testEventEventStore
            };
            entityStremRepo.Setup(ev => ev.LoadEventsByEntity(It.IsAny<Guid>(), It.IsAny<long>()))
                .ReturnsAsync( Result<IEnumerable<DomainEventWrapper>>.Ok( new[] { domainEventWrapper }));
            var eventStore = new EventStore(entityStremRepo.Object, new EventSourcingApplyStrategy());
            var loadAsync = await eventStore.LoadAsync<TestEntity_NoIApply>(entityId);

            Assert.AreEqual(new Guid(), loadAsync.Entity.Id);
        }
    }

    public class TestEntity_WrongIApply : IApply<WrongEvent>
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

    public class TestEntity_NoIApply
    {
        public Guid Id { get; private set; }
    }

    public class TestEntity : IApply<TestEventEventStore>
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