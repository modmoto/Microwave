using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.Persistence.MongoDb.EventStores;
using Microwave.Persistence.MongoDb.UnitTests.Eventstores;
using Microwave.WebApi.Querries;

namespace Microwave.Eventstores.UnitTests
{
    [TestClass]
    public class ReadStoreTests : IntegrationTests
    {
        private readonly IDomainEventFactory _domainEventFactory = new DomainEventFactory(new EventRegistration
        {
            { nameof(TestEv), typeof(TestEv) },
            { nameof(TestEv_AutoProperty), typeof(TestEv_AutoProperty) },
            { nameof(TestEv_CustomBackingField), typeof(TestEv_CustomBackingField) },
            { nameof(TestEv_DifferentParamName), typeof(TestEv_DifferentParamName) }
        });

        [TestMethod]
        public void TestDeserializationOfIdInInterface_DifferentParameterNameList()
        {
            var domainEvent = new TestEv_DifferentParamName(GuidIdentity.Create(new Guid("48eb878a-4483-40d9-bf4f-36c85ba5f803")), "testString");
            var serialize =
                "[      {   \"created\":\"2018-01-07T19:07:21.227631+01:00\",       \"version\":14,   \"domainEventType\":\"TestEv_DifferentParamName\",         \"domainEvent\":{    \"secondProp\":\"testString\",           \"entityId\":{\"Id\": \"48eb878a-4483-40d9-bf4f-36c85ba5f803\" }  }     }   ]";
            var deserialize = new DomainEventWrapperListDeserializer(new JSonHack(), _domainEventFactory).Deserialize(serialize).First().DomainEvent;
            Assert.AreEqual(domainEvent.EntityId.Id, deserialize.EntityId.Id);
            Assert.AreEqual(((TestEv_DifferentParamName)deserialize).SecondProp, "testString");
            Assert.AreNotEqual(deserialize.EntityId, Guid.Empty);
        }

        [TestMethod]
        public void TestDeserializationOfIdInInterface_DifferentParameterNameList_TwoEntries()
        {
            var domainEvent = new TestEv_DifferentParamName(GuidIdentity.Create(new Guid("84e5447a-0a28-4fe1-af5a-11dd6a43d3dd")), "testString");
            var domainEvent2 = new TestEv_DifferentParamName(GuidIdentity.Create(new Guid("48eb878a-4483-40d9-bf4f-36c85ba5f803")), "andererString");
            var serialize =
                "[      {   \"created\":\"2018-01-07T19:07:21.227631+01:00\",       \"version\":14, \"domainEventType\":\"TestEv_DifferentParamName\",         \"domainEvent\":{    \"secondProp\":\"testString\",           \"entityId\":{\"Id\": \"84e5447a-0a28-4fe1-af5a-11dd6a43d3dd\" }  }     },   {   \"created\":\"2018-01-07T19:07:23.227631+01:00\",       \"version\":14,    \"domainEventType\":\"TestEv_DifferentParamName\",        \"domainEvent\":{    \"secondProp\":\"andererString\",           \"entityId\":{\"Id\": \"48eb878a-4483-40d9-bf4f-36c85ba5f803\"}   }     }   ]";
            var domainEventWrappers = new DomainEventWrapperListDeserializer(new JSonHack(), _domainEventFactory).Deserialize(serialize).ToList();
            var deserialize = domainEventWrappers[0].DomainEvent;
            var deserialize2 = domainEventWrappers[1].DomainEvent;
            Assert.AreEqual(domainEvent.EntityId.Id, deserialize.EntityId.Id);
            Assert.AreEqual(domainEvent2.EntityId.Id, deserialize2.EntityId.Id);
            Assert.AreEqual(((TestEv_DifferentParamName)deserialize).SecondProp, "testString");
            Assert.AreEqual(((TestEv_DifferentParamName)deserialize2).SecondProp, "andererString");
            Assert.AreNotEqual(deserialize.EntityId.Id, Guid.Empty.ToString());
            Assert.AreNotEqual(deserialize2.EntityId.Id, Guid.Empty.ToString());
        }

        [TestMethod]
        public void TestDeserializationOfIdInInterface_DifferentParameterNameList_TwoEntries_MixedCamelCaseAndPascalCase()
        {
            var domainEvent = new TestEv_DifferentParamName(GuidIdentity.Create(new Guid("84e5447a-0a28-4fe1-af5a-11dd6a43d3dd")),
            "testString");
            var domainEvent2 = new TestEv_DifferentParamName(GuidIdentity.Create(new Guid("48eb878a-4483-40d9-bf4f-36c85ba5f803")), "andererString");
            var serialize =
                "[      {   \"created\":\"2018-01-07T19:07:21.227631+01:00\",       \"version\":14,    \"domainEventType\":\"TestEv_DifferentParamName\",        \"DomainEvent\":{    \"secondProp\":\"testString\",           \"entityId\":{\"Id\": \"84e5447a-0a28-4fe1-af5a-11dd6a43d3dd\" }  }     },   {   \"created\":\"2018-01-07T19:07:21.227631+01:00\",       \"version\":14,      \"domainEventType\":\"TestEv_DifferentParamName\",      \"domainEvent\":{    \"secondProp\":\"andererString\",           \"EntityId\":{\"Id\": \"48eb878a-4483-40d9-bf4f-36c85ba5f803\"  } }     }   ]";
            var domainEventWrappers = new DomainEventWrapperListDeserializer(new JSonHack(), _domainEventFactory).Deserialize(serialize).ToList();
            var deserialize = domainEventWrappers[0].DomainEvent;
            var deserialize2 = domainEventWrappers[1].DomainEvent;
            Assert.AreEqual(domainEvent.EntityId.Id, deserialize.EntityId.Id);
            Assert.AreEqual(domainEvent2.EntityId.Id, deserialize2.EntityId.Id);
            Assert.AreEqual(((TestEv_DifferentParamName)deserialize).SecondProp, "testString");
            Assert.AreEqual(((TestEv_DifferentParamName)deserialize2).SecondProp, "andererString");
            Assert.AreNotEqual(deserialize.EntityId.Id, Guid.Empty.ToString());
            Assert.AreNotEqual(deserialize2.EntityId.Id, Guid.Empty.ToString());
        }

        [TestMethod]
        public async Task Entitystream_LoadEventsSince_IdNotDefault()
        {
            var entityStreamRepository = new EventRepository(EventDatabase, new VersionCache(EventDatabase));

            var entityStreamTestEvent = new TestEv3(GuidIdentity.Create(Guid.NewGuid()));
            await entityStreamRepository.AppendAsync(new[] {entityStreamTestEvent}, 0);

            var eventsSince = await entityStreamRepository.LoadEvents();

            Assert.AreEqual(entityStreamTestEvent.EntityId.Id, eventsSince.Value.Single().DomainEvent.EntityId.Id);
            Assert.AreNotEqual(entityStreamTestEvent.EntityId.Id, Guid.Empty.ToString());
        }
    }

    public class TestEv3 : IDomainEvent
    {
        public TestEv3(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }

    public class TestEv : IDomainEvent
    {
        public TestEv(Identity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }

    public class TestEv_DifferentParamName : IDomainEvent
    {
        public TestEv_DifferentParamName(Identity NOTentityId, string secondProp)
        {
            EntityId = NOTentityId;
            SecondProp = secondProp;
        }

        public string SecondProp { get; }

        public Identity EntityId { get; }
    }
}