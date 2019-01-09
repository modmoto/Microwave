using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.EventStores;

namespace Microwave.Eventstores.UnitTests
{
    [TestClass]
    public class MongoDbTests : IntegrationTests
    {
        // This is not supported and might never be
        [TestMethod]
        public async Task TestDeserializationOfIdInInterface_OwnBackingField()
        {
            var entityStreamRepository = new EventRepository(EventDatabase, new VersionCache(EventDatabase));
            var domainEvent = new TestEv_CustomBackingField(Guid.NewGuid().ToString());

            await entityStreamRepository.AppendAsync(new List<IDomainEvent> {domainEvent}, 0);
            var deserialize = (await entityStreamRepository.LoadEvents()).Value.Single().DomainEvent;
            Assert.IsNull(deserialize.EntityId);
        }

        [TestMethod]
        public async Task TestDeserializationOfIdInInterface_GetAutoProperty()
        {
            var entityStreamRepository = new EventRepository(EventDatabase, new VersionCache(EventDatabase));

            var domainEvent = new TestEv_AutoProperty(Guid.NewGuid().ToString());
            await entityStreamRepository.AppendAsync(new List<IDomainEvent> {domainEvent}, 0);
            var deserialize = (await entityStreamRepository.LoadEvents()).Value.Single().DomainEvent;
            Assert.AreEqual(deserialize.EntityId, new Guid("84e5447a-0a28-4fe1-af5a-11dd6a43d3dd").ToString());
            Assert.AreNotEqual(deserialize.EntityId, Guid.Empty);
        }

        [TestMethod]
        public async Task TestDeserializationOfIdInInterface()
        {
            var entityStreamRepository = new EventRepository(EventDatabase, new VersionCache(EventDatabase));

            var domainEvent = new TestEv2(Guid.NewGuid().ToString());
            await entityStreamRepository.AppendAsync(new List<IDomainEvent> {domainEvent}, 0);
            var deserialize = (await entityStreamRepository.LoadEvents()).Value.Single().DomainEvent;

            Assert.IsTrue(deserialize.EntityId.Equals(domainEvent.EntityId));
            Assert.AreNotEqual(deserialize.EntityId, Guid.Empty.ToString());
        }
    }

    public class TestEv2 : IDomainEvent
    {
        public TestEv2(string entityId)
        {
            EntityId = entityId;
        }

        public string EntityId { get; }
    }

    public class TestEv_CustomBackingField : IDomainEvent
    {
        private readonly string _entityIdBackingFieldWeird;

        public TestEv_CustomBackingField(string NOTentityId)
        {
            _entityIdBackingFieldWeird = NOTentityId;
        }

        public string EntityId => _entityIdBackingFieldWeird;
    }

    public class TestEv_AutoProperty : IDomainEvent
    {
        public TestEv_AutoProperty(string NOTentityId)
        {
        }

        public string EntityId => new Guid("84e5447a-0a28-4fe1-af5a-11dd6a43d3dd").ToString();
    }
}