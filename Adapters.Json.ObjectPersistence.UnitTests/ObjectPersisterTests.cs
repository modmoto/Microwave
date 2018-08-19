using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Adapters.Json.ObjectPersistences;
using Domain.Framework;
using Xunit;

namespace Adapters.Json.ObjectPersistence.UnitTests
{
    public class ObjectPersisterTests
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