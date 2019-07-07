using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.Persistence.UnitTestSetupPorts;

namespace Microwave.Persistence.UnitTests.Eventstores
{
    [TestClass]
    public class EventRepositoryDeserialisationTests
    {
        // This is not supported and might never be
        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task TestDeserializationOfIdInInterface_OwnBackingField(IPersistenceLayerProvider layerProvider)
        {
            var entityStreamRepository = layerProvider.EventRepository;
            var domainEvent = new TestEv_CustomBackingField(GuidIdentity.Create(Guid.NewGuid()));

            await entityStreamRepository.AppendAsync(new List<IDomainEvent> {domainEvent}, 0);
            var deserialize = (await entityStreamRepository.LoadEvents()).Value.Single().DomainEvent;
            Assert.IsNull(deserialize.EntityId);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task TestDeserializationOfIdInInterface_GetAutoProperty(IPersistenceLayerProvider layerProvider)
        {
            var entityStreamRepository = layerProvider.EventRepository;

            var domainEvent = new TestEv_AutoProperty(GuidIdentity.Create(Guid.NewGuid()));
            await entityStreamRepository.AppendAsync(new List<IDomainEvent> {domainEvent}, 0);
            var deserialize = (await entityStreamRepository.LoadEvents()).Value.Single().DomainEvent;
            Assert.AreEqual(deserialize.EntityId.Id, new Guid("84e5447a-0a28-4fe1-af5a-11dd6a43d3dd").ToString());
            Assert.AreNotEqual(deserialize.EntityId, Guid.Empty);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task TestDeserializationOfIdInInterface(IPersistenceLayerProvider layerProvider)
        {
            var entityStreamRepository = layerProvider.EventRepository;

            var domainEvent = new TestEv2(GuidIdentity.Create(Guid.NewGuid()));
            await entityStreamRepository.AppendAsync(new List<IDomainEvent> {domainEvent}, 0);
            var deserialize = (await entityStreamRepository.LoadEvents()).Value.Single().DomainEvent;

            Assert.IsTrue(deserialize.EntityId.Equals(domainEvent.EntityId));
            Assert.AreNotEqual(deserialize.EntityId.Id, Guid.Empty.ToString());
        }
    }

    public class TestEv2 : IDomainEvent
    {
        public TestEv2(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }

    public class TestEv_CustomBackingField : IDomainEvent
    {
        private readonly Identity _entityIdBackingFieldWeird;

        public TestEv_CustomBackingField(Identity NOTentityId)
        {
            _entityIdBackingFieldWeird = NOTentityId;
        }

        public Identity EntityId => _entityIdBackingFieldWeird;
    }

    public class TestEv_AutoProperty : IDomainEvent
    {
        public TestEv_AutoProperty(Identity NOTentityId)
        {
        }

        public Identity EntityId => GuidIdentity.Create(new Guid("84e5447a-0a28-4fe1-af5a-11dd6a43d3dd"));
    }
}