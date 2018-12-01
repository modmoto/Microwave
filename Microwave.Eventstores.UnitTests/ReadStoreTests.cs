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
            var domainEvent = new TestEv(Guid.NewGuid());
            var serialize = objectConverter.Serialize(domainEvent);
            var deserialize = objectConverter.Deserialize<IDomainEvent>(serialize);
            Assert.AreEqual(deserialize.EntityId, domainEvent.EntityId);
            Assert.AreNotEqual(deserialize.EntityId, new Guid());
        }

        [Test]
        public void TestDeserializationOfIdInInterface_DifferentParameterName()
        {
            var objectConverter = new ObjectConverter();
            var domainEvent = new TestEv_DifferentParamName(Guid.NewGuid(), "testString");
            var serialize = objectConverter.Serialize(domainEvent);
            var deserialize = (TestEv_DifferentParamName) objectConverter.Deserialize<IDomainEvent>(serialize);
            Assert.AreEqual(deserialize.EntityId, domainEvent.EntityId);
            Assert.AreEqual(deserialize.SecondProp, "testString");
            Assert.AreNotEqual(deserialize.EntityId, new Guid());
        }

        // This is not supported and might never be
        [Test]
        public void TestDeserializationOfIdInInterface_OwnBackingField()
        {
            var objectConverter = new ObjectConverter();
            var domainEvent = new TestEv_CustomBackingField(Guid.NewGuid());
            var serialize = objectConverter.Serialize(domainEvent);
            var deserialize = objectConverter.Deserialize<IDomainEvent>(serialize);
            Assert.AreEqual(deserialize.EntityId, new Guid());
        }

        [Test]
        public void TestDeserializationOfIdInInterface_GetAutoProperty()
        {
            var objectConverter = new ObjectConverter();
            var domainEvent = new TestEv_AutoProperty(Guid.NewGuid());
            var serialize = objectConverter.Serialize(domainEvent);
            var deserialize = objectConverter.Deserialize<IDomainEvent>(serialize);
            Assert.AreEqual(deserialize.EntityId, new Guid("84e5447a-0a28-4fe1-af5a-11dd6a43d3dd"));
            Assert.AreNotEqual(deserialize.EntityId, new Guid());
        }

        [Test]
        public async Task Entitystream_LoadEventsSince_IdNotDefault()
        {
            var optionsRead = new DbContextOptionsBuilder<EventStoreWriteContext>()
                .UseInMemoryDatabase("Entitystream_LoadEventsSince_IdNotDefault")
                .Options;

            var entityStreamRepository = new EntityStreamRepository(new ObjectConverter(), new EventStoreWriteContext(optionsRead));

            var entityStreamTestEvent = new TestEv(Guid.NewGuid());
            await entityStreamRepository.AppendAsync(new[] {entityStreamTestEvent}, -1);

            var eventsSince = await entityStreamRepository.LoadEventsSince();

            Assert.AreEqual(entityStreamTestEvent.EntityId, eventsSince.Value.Single().DomainEvent.EntityId);
            Assert.AreNotEqual(entityStreamTestEvent.EntityId, new Guid());
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

    public class TestEv_DifferentParamName : IDomainEvent
    {
        public TestEv_DifferentParamName(Guid NOTentityId, string secondProp)
        {
            EntityId = NOTentityId;
            SecondProp = secondProp;
        }

        public Guid EntityId { get; }
        public string SecondProp { get; }
    }

    public class TestEv_CustomBackingField : IDomainEvent
    {
        private readonly Guid _WeirdKrappyName;

        public TestEv_CustomBackingField(Guid NOTentityId)
        {
            _WeirdKrappyName = NOTentityId;
        }

        public Guid EntityId => _WeirdKrappyName;
    }

    public class TestEv_AutoProperty : IDomainEvent
    {
        public TestEv_AutoProperty(Guid NOTentityId)
        {
        }

        public Guid EntityId => new Guid("84e5447a-0a28-4fe1-af5a-11dd6a43d3dd");
    }
}