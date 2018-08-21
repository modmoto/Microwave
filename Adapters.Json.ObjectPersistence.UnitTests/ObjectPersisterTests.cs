using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Adapters.Json.ObjectPersistences;
using Domain.Framework;
using Xunit;

namespace Adapters.Json.ObjectPersistence.UnitTests
{
    public class ObjectPersisterTests
    {
        private string _jsonDb = "DB_IEnumerable`1_System.Collections.Generic.IEnumerable`1[[Domain.Framework.DomainEvent, Domain.Framework, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]].json";

        [Fact]
        public async Task SaveEvents()
        {
            var entityId = Guid.NewGuid();
            var domainEvents = new List<DomainEvent> { new TestEvent(entityId, "TestSession1"), new TestEvent(entityId, "TestSession2")};
            var domainObjectPersister = new DomainEventPersister();
            await domainObjectPersister.Save(domainEvents);
            var savedEvents = (await domainObjectPersister.GetAsync()).ToList();

            Assert.Equal(savedEvents[0].EntityId, domainEvents[0].EntityId);
            Assert.Equal(((TestEvent)savedEvents[0]).Name, ((TestEvent)domainEvents[0]).Name);
            Assert.Equal(savedEvents[1].EntityId, domainEvents[1].EntityId);
            Assert.Equal(((TestEvent)savedEvents[1]).Name, ((TestEvent)domainEvents[1]).Name);
        }

        [Fact]
        public async Task GetEvents_UnknownEvent()
        {
            File.Copy(_jsonDb, $"JsonDb/{_jsonDb}", true);
            var domainObjectPersister = new DomainEventPersister();
            await domainObjectPersister.GetAsync();
            var savedEvents = (await domainObjectPersister.GetAsync()).ToList();

            Assert.Equal(new Guid("c3381252-f498-4585-95d8-faf8af00854b"), savedEvents[0].Id);
            Assert.Equal(new Guid("06b2b403-6ed0-4bc0-a119-72ef63a571e3"), savedEvents[0].EntityId);
            Assert.Equal("TestSession1", ((TestEvent)savedEvents[0]).Name);
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