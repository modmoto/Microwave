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
    }

    internal class TestChangeNameReflectionEvent : DomainEvent
    {
        [PropertyPath("Name")]
        public string NewName { get; }

        public TestChangeNameReflectionEvent(Guid entityId, string newName) : base(entityId)
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

    internal class TestReflectionEntity : Entity
    {
        public string Name { get; private set; }
    }
}