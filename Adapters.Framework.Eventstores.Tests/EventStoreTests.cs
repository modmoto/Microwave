using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adapters.Framework.EventStores;
using Application.Framework;
using Domain.Framework;
using Moq;
using Xunit;

namespace Adapters.Framework.Eventstores.Tests
{
    public class EventStoreTests
    {
        [Fact]
        public async Task AppendAsync_List_NoEventsPersisted()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent> { new TestEvent(entityId, "TestSession1"), new TestEvent(entityId, "TestSession2")};

            var persister = new Mock<IDomainEventPersister>();
            persister.Setup(per => per.GetAsync()).ReturnsAsync(default(IEnumerable<DomainEvent>));

            var eventStore = new EventStore(persister.Object, new EventSourcingAtributeStrategy());
            await eventStore.AppendAsync(domainEvents);

            Assert.Equal(2, (await eventStore.GetEvents()).Count());
        }

        [Fact]
        public async Task AppendAsync_List_EventsAllreadyPresent()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent> { new TestEvent(entityId, "TestSession1"), new TestEvent(entityId, "TestSession2")};

            var persister = new Mock<IDomainEventPersister>();
            persister.Setup(per => per.GetAsync()).ReturnsAsync(domainEvents);

            var eventStore = new EventStore(persister.Object, new EventSourcingApplyStrategy());
            await eventStore.AppendAsync(domainEvents);

            Assert.Equal(4, (await eventStore.GetEvents()).Count());
        }

        [Fact]
        public async Task AppendAsync_SingleEvent()
        {
            var testEvent = new TestEvent(Guid.NewGuid(), "TestSession2");

            var persister = new Mock<IDomainEventPersister>();
            persister.Setup(per => per.GetAsync()).ReturnsAsync(new List<DomainEvent>());

            var eventStore = new EventStore(persister.Object, new EventSourcingApplyStrategy());
            await eventStore.AppendAsync(testEvent);

            Assert.Equal(1, (await eventStore.GetEvents()).Count());
        }

        [Fact]
        public async Task ApplyStrategy()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent> { new TestCreatedEvent(entityId, "OldName"), new TestChangeNameEvent(entityId, "NewName")};

            var persister = new Mock<IDomainEventPersister>();
            persister.Setup(per => per.GetAsync()).ReturnsAsync(domainEvents);

            var eventStore = new EventStore(persister.Object, new EventSourcingApplyStrategy());
            var testEntity = await eventStore.LoadAsync<TestEntity>(entityId);

            Assert.Equal("NewName", testEntity.Name);
            Assert.Equal(entityId, testEntity.Id);
        }
    }

    internal class TestChangeNameEvent : DomainEvent
    {
        public string NewName { get; }

        public TestChangeNameEvent(Guid entityId, string newName) : base(entityId)
        {
            NewName = newName;
        }
    }

    internal class TestCreatedEvent : DomainEvent
    {
        public string Name { get; }

        public TestCreatedEvent(Guid entityId, string name) : base(entityId)
        {
            Name = name;
        }
    }

    internal class TestEntity : Entity
    {
        public void Apply(TestCreatedEvent domainEvent)
        {
            Id = domainEvent.EntityId;
            Name = domainEvent.Name;
        }

        public void Apply(TestChangeNameEvent domainEvent)
        {
            Name = domainEvent.NewName;
        }

        public string Name { get; private set; }
    }

    internal class TestEvent : DomainEvent
    {
        public string Name { get; }

        public TestEvent(Guid guid, string name) : base(guid)
        {
            Name = name;
        }
    }
}