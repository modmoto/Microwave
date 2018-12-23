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
            var entityStreamRepository = new EventRepository(new EventDatabase(Database));
            var domainEvent = new TestEv_CustomBackingField(Guid.NewGuid());

            await entityStreamRepository.AppendAsync(new List<IDomainEvent> {domainEvent}, 0);
            var deserialize = (await entityStreamRepository.LoadEvents()).Value.Single().DomainEvent;
            Assert.AreEqual(deserialize.EntityId, Guid.Empty);
        }

        [TestMethod]
        public async Task TestDeserializationOfIdInInterface_GetAutoProperty()
        {
            var entityStreamRepository = new EventRepository(new EventDatabase(Database));

            var domainEvent = new TestEv_AutoProperty(Guid.NewGuid());
            await entityStreamRepository.AppendAsync(new List<IDomainEvent> {domainEvent}, 0);
            var deserialize = (await entityStreamRepository.LoadEvents()).Value.Single().DomainEvent;
            Assert.AreEqual(deserialize.EntityId, new Guid("84e5447a-0a28-4fe1-af5a-11dd6a43d3dd"));
            Assert.AreNotEqual(deserialize.EntityId, Guid.Empty);
        }

        [TestMethod]
        public async Task TestDeserializationOfIdInInterface()
        {
            var entityStreamRepository = new EventRepository(new EventDatabase(Database));

            var domainEvent = new TestEv(Guid.NewGuid());
            await entityStreamRepository.AppendAsync(new List<IDomainEvent> {domainEvent}, 0);
            var deserialize = (await entityStreamRepository.LoadEvents()).Value.Single().DomainEvent;

            Assert.AreEqual(deserialize.EntityId, domainEvent.EntityId);
            Assert.AreNotEqual(deserialize.EntityId, Guid.Empty);
        }
    }

    public class TestEv_CustomBackingField : IDomainEvent
    {
        private readonly Guid _entityIdBackingFieldWeird;

        public TestEv_CustomBackingField(Guid NOTentityId)
        {
            _entityIdBackingFieldWeird = NOTentityId;
        }

        public Guid EntityId => _entityIdBackingFieldWeird;
    }

    public class TestEv_AutoProperty : IDomainEvent
    {
        public TestEv_AutoProperty(Guid NOTentityId)
        {
        }

        public Guid EntityId => new Guid("84e5447a-0a28-4fe1-af5a-11dd6a43d3dd");
    }
}