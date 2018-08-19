using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Adapters.Framework.EventStores;
using Domain.Framework;
using Domain.Seasons;
using Xunit;

namespace Adapters.Framework.Eventstores.Tests
{
    public class EventStoreTests
    {
        private string _filePath;

        [Fact]
        public async Task SaveEvents()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent> { new TestEvent(entityId, "TestSession1"), new TestEvent(entityId, "TestSession2")};
            _filePath = "test.txt";
            var domainObjectPersister = new DomainObjectPersister(_filePath);
            await domainObjectPersister.Store(domainEvents);
            var savedEvents = domainObjectPersister.Read().ToList();

            Assert.Equal(savedEvents[0].EntityId, domainEvents[0].EntityId);
            Assert.Equal(((TestEvent)savedEvents[0]).Name, ((TestEvent)domainEvents[0]).Name);
            Assert.Equal(savedEvents[1].EntityId, domainEvents[1].EntityId);
            Assert.Equal(((TestEvent)savedEvents[1]).Name, ((TestEvent)domainEvents[1]).Name);
        }
    }

    public class TestEvent : DomainEvent
    {
        public string Name { get; }

        public TestEvent(Guid guid, string name) : base(guid)
        {
            Name = name;
        }
    }
}