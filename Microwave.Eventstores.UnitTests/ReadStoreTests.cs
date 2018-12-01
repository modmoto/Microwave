using System;
using Microwave.Domain;
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
        }   
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