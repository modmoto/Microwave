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
        [Fact]
        public async Task SaveAndLoadEvents()
        {
            var entityId = Guid.NewGuid();
            var domainEvent = new TestEvent(entityId);
            domainEvent.SetName("TestSession1");
            var domainEvents = new List<DomainEvent> { domainEvent, new TestEventOld(entityId, "TestSession2", "TestSession3")};
            var domainObjectPersister = new DomainEventPersister();
            await domainObjectPersister.Save(domainEvents);
            var savedEvents = (await domainObjectPersister.GetAsync()).ToList();

            Assert.Equal(savedEvents[0].EntityId, domainEvents[0].EntityId);
            Assert.Equal(((TestEvent)domainEvents[0]).Name, ((TestEvent)savedEvents[0]).Name);
            Assert.Equal(savedEvents[1].EntityId, domainEvents[1].EntityId);
            Assert.Equal(((TestEventOld)domainEvents[1]).Name, ((TestEventOld)savedEvents[1]).Name);
        }
    }

    internal class TestEvent : DomainEvent
    {
        public string Name { get; private set; }

        public TestEvent(Guid guid) : base(guid)
        {
        }

        public void SetName(string name)
        {
            Name = name;
        }
    }

    internal class TestEventOld : DomainEvent
    {
        public string Name { get; }
        public string LastName { get; }

        public TestEventOld(Guid guid, string lastName, string name) : base(guid)
        {
            Name = name;
            LastName = lastName;
        }
    }
}