using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adapters.Framework.EventStores;
using Domain.Framework;
using Moq;
using Xunit;

namespace Adapters.Framework.Eventstores.Tests
{
    public class EventStoreTests
    {
        private string _filePath = "test.txt";

        [Fact]
        public async Task SaveEvents()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent> { new TestEvent(entityId, "TestSession1"), new TestEvent(entityId, "TestSession2")};
            var domainObjectPersister = new DomainEventPersister(_filePath);
            await domainObjectPersister.Store(domainEvents);
            var savedEvents = domainObjectPersister.Load().ToList();

            Assert.Equal(savedEvents[0].EntityId, domainEvents[0].EntityId);
            Assert.Equal(((TestEvent)savedEvents[0]).Name, ((TestEvent)domainEvents[0]).Name);
            Assert.Equal(savedEvents[1].EntityId, domainEvents[1].EntityId);
            Assert.Equal(((TestEvent)savedEvents[1]).Name, ((TestEvent)domainEvents[1]).Name);
        }

        [Fact]
        public async Task AppendAsync_List()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent> { new TestEvent(entityId, "TestSession1"), new TestEvent(entityId, "TestSession2")};

            var persister = new Mock<IDomainEventPersister>();
            persister.Setup(per => per.Load()).Returns(domainEvents);

            var eventStore = new EventStore(persister.Object);
            await eventStore.AppendAsync(domainEvents);

            Assert.Equal(2, eventStore.DomainEvents.Count());
        }

        [Fact]
        public async Task AppendAsync_SingleEvent()
        {
            var testEvent = new TestEvent(Guid.NewGuid(), "TestSession2");

            var persister = new Mock<IDomainEventPersister>();
            persister.Setup(per => per.Load()).Returns(new List<DomainEvent> { testEvent });

            var eventStore = new EventStore(persister.Object);
            await eventStore.AppendAsync(testEvent);

            Assert.Equal(1, eventStore.DomainEvents.Count());
        }

        [Fact]
        public void LoadEntity()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent> { new TestCreatedEvent(entityId, "OldName"), new TestChangeNameEvent(entityId, "NewName")};

            var persister = new Mock<IDomainEventPersister>();
            persister.Setup(per => per.Load()).Returns(domainEvents);

            var eventStore = new EventStore(persister.Object);
            var testEntity = eventStore.LoadAsync<TestEntity>(entityId);

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