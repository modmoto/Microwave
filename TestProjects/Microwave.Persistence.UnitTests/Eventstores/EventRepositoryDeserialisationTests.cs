using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Persistence.UnitTestsSetup;

namespace Microwave.Persistence.UnitTests.Eventstores
{
    [TestClass]
    public class EventRepositoryDeserialisationTests
    {
        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task TestDeserializationOfIdInInterface_GetAutoProperty(PersistenceLayerProvider layerProvider)
        {
            var entityStreamRepository = layerProvider.EventRepository;

            var domainEvent = new TestEv_AutoProperty(Guid.NewGuid());
            await entityStreamRepository.AppendAsync(new List<IDomainEvent> {domainEvent}, 0);
            var deserialize = (await entityStreamRepository.LoadEvents()).Value.Single().DomainEvent;
            Assert.AreEqual(deserialize.EntityId, new Guid("84e5447a-0a28-4fe1-af5a-11dd6a43d3dd").ToString());
            Assert.AreNotEqual(deserialize.EntityId, Guid.Empty);
        }

        [DataTestMethod]
        [PersistenceTypeTest]
        public async Task TestDeserializationOfIdInInterface(PersistenceLayerProvider layerProvider)
        {
            var entityStreamRepository = layerProvider.EventRepository;

            var domainEvent = new TestEv2(Guid.NewGuid());
            await entityStreamRepository.AppendAsync(new List<IDomainEvent> {domainEvent}, 0);
            var deserialize = (await entityStreamRepository.LoadEvents()).Value.Single().DomainEvent;

            Assert.AreEqual(deserialize.EntityId, domainEvent.EntityId);
            Assert.AreNotEqual(deserialize.EntityId, Guid.Empty.ToString());
        }
    }

    public class TestEv2 : IDomainEvent
    {
        public Guid TypeId { get; }
        public TestEv2(Guid typeId)
        {
            TypeId = typeId;
        }

        public string EntityId => TypeId.ToString();
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
        public TestEv_AutoProperty(Guid NOTentityId)
        {
        }

        public string EntityId => new Guid("84e5447a-0a28-4fe1-af5a-11dd6a43d3dd").ToString();
    }
}