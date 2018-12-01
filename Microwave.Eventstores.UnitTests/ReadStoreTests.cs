using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microwave.Domain;
using Microwave.EventStores;
using Microwave.ObjectPersistences;
using NUnit.Framework;

namespace Microwave.Eventstores.UnitTests
{
    public class ReadStoreTests
    {
        [Test]
        public void TestDeserializationOfIdInInterface()
        {
            var objectConverter = new ObjectConverter();
            TestEv domainEvent = new TestEv(Guid.NewGuid());
            var serialize = objectConverter.Serialize(domainEvent);
            var deserialize = objectConverter.Deserialize<IDomainEvent>(serialize);
            Assert.AreEqual(deserialize.EntityId, domainEvent.EntityId);
            Assert.AreNotEqual(deserialize.EntityId, new Guid());
        }

        [Test]
        public async Task Entitystream_LoadEventsSince_IdNotDefault()
        {
            var optionsRead = new DbContextOptionsBuilder<EventStoreWriteContext>()
                .UseInMemoryDatabase("Entitystream_LoadEventsSince_IdNotDefault")
                .Options;

            var entityStreamRepository = new EntityStreamRepository(new ObjectConverter(), new EventStoreWriteContext(optionsRead));

            var entityStreamTestEvent = new TestEv_EntityStream(Guid.NewGuid());
            await entityStreamRepository.AppendAsync(new[] {entityStreamTestEvent}, -1);

            var eventsSince = await entityStreamRepository.LoadEventsSince();

            Assert.AreEqual(entityStreamTestEvent.EntityId, eventsSince.Value.Single().DomainEvent.EntityId);
            Assert.AreNotEqual(entityStreamTestEvent.EntityId, new Guid());
        }
    }

    public class TestEv_EntityStream : IDomainEvent
    {
        public TestEv_EntityStream(Guid newGuid)
        {
            EntityId = newGuid;
        }

        public Guid EntityId { get; }
    }

    public class TestEv : IDomainEvent
    {
        public TestEv(Guid entityId)
        {
            EntityId = entityId;
        }

        public Guid EntityId { get; }
    }
}