using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Adapters.Framework.EventStores;
using Application.Framework;
using Domain.Framework;
using Moq;
using Xunit;

namespace Adapters.Framework.Eventstores.Tests
{
    public class ReflectionEventStoreTests
    {
        [Fact]
        public async Task LoadEntity()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent> { new TestCreatedReflectionEvent(entityId, "OldName"), new TestChangeNameReflectionEvent(entityId, "NewName")};

            var persister = new Mock<IDomainEventPersister>();
            persister.Setup(per => per.GetAsync()).ReturnsAsync(domainEvents);

            var eventStore = new ReflectionEventStore(persister.Object);
            var testEntity = await eventStore.LoadAsync<TestReflectionEntity>(entityId);

            Assert.Equal("NewName", testEntity.Name);
            Assert.Equal(entityId, testEntity.Id);
        }

        [Fact]
        public async Task LoadEntity_OldUnusedPropIsCalled()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent> { new TestCreatedReflectionEvent(entityId, "OldName"), new TestCreatedReflectionEventOldLastNameEvent(entityId, "Old entity Name")};

            var persister = new Mock<IDomainEventPersister>();
            persister.Setup(per => per.GetAsync()).ReturnsAsync(domainEvents);

            var eventStore = new ReflectionEventStore(persister.Object);
            var testEntity = await eventStore.LoadAsync<TestReflectionEntity>(entityId);

            Assert.Equal("OldName", testEntity.Name);
            Assert.Equal(entityId, testEntity.Id);
        }

        [Fact]
        public async Task LoadEntity_PropNotExisting()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent> { new TestCreatedReflectionEvent(entityId, "OldName"), new TestChangeNameReflectionEventPropNotExisting(entityId, "NewName")};

            var persister = new Mock<IDomainEventPersister>();
            persister.Setup(per => per.GetAsync()).ReturnsAsync(domainEvents);

            var eventStore = new ReflectionEventStore(persister.Object);
            var throwsAsync = await Assert.ThrowsAsync<ApplicationException>(async () => await eventStore.LoadAsync<TestReflectionEntity>(entityId));
            Assert.Equal("Property ThisPropIsWrong does not exist on entity, check the ActualPropertyName Attribute on Property NewName of Event TestChangeNameReflectionEventPropNotExisting", throwsAsync.Message);
        }
    }

    internal class TestChangeNameReflectionEvent : DomainEvent
    {
        [ActualPropertyName("Name")]
        public string NewName { get; }

        public TestChangeNameReflectionEvent(Guid entityId, string newName) : base(entityId)
        {
            NewName = newName;
        }
    }

    internal class TestChangeNameReflectionEventPropNotExisting : DomainEvent
    {
        [ActualPropertyName("ThisPropIsWrong")]
        public string NewName { get; }

        public TestChangeNameReflectionEventPropNotExisting(Guid entityId, string newName) : base(entityId)
        {
            NewName = newName;
        }
    }

    internal class TestCreatedReflectionEvent : DomainEvent
    {
        public string Name { get; }

        public TestCreatedReflectionEvent(Guid entityId, string name) : base(entityId)
        {
            Name = name;
        }
    }

    internal class TestCreatedReflectionEventOldLastNameEvent : DomainEvent
    {
        public string LastName { get; }

        public TestCreatedReflectionEventOldLastNameEvent(Guid entityId, string name) : base(entityId)
        {
            LastName = name;
        }
    }

    internal class TestReflectionEntity : Entity
    {
        //TODO make this private set shit better
        public string Name { get; set; }
    }
}