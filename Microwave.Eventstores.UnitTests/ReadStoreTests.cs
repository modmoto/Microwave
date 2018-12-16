using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.EventStores;
using Microwave.ObjectPersistences;
using Mongo2Go;
using MongoDB.Driver;

namespace Microwave.Eventstores.UnitTests
{
    [TestClass]
    public class ReadStoreTests
    {
        private readonly DomainEventFactory _domainEventFactory = new DomainEventFactory(new EventRegistration
        {
            { nameof(TestEv), typeof(TestEv) },
            { nameof(TestEv_AutoProperty), typeof(TestEv_AutoProperty) },
            { nameof(TestEv_CustomBackingField), typeof(TestEv_CustomBackingField) },
            { nameof(TestEv_DifferentParamName), typeof(TestEv_DifferentParamName) }
        });

        [TestMethod]
        public void TestDeserializationOfIdInInterface()
        {
            var objectConverter = new ObjectConverter();
            var domainEvent = new TestEv(Guid.NewGuid());
            var serialize = objectConverter.Serialize(domainEvent);
            var deserialize = objectConverter.Deserialize<IDomainEvent>(serialize);
            Assert.AreEqual(deserialize.EntityId, domainEvent.EntityId);
            Assert.AreNotEqual(deserialize.EntityId, Guid.Empty);
        }

        [TestMethod]
        public void TestDeserializationOfIdInInterface_DifferentParameterName()
        {
            var objectConverter = new ObjectConverter();
            var domainEvent = new TestEv_DifferentParamName(Guid.NewGuid(), "testString");
            var serialize = objectConverter.Serialize(domainEvent);
            var deserialize = (TestEv_DifferentParamName) new DomainEventDeserializer(new JSonHack()).Deserialize(serialize);
            Assert.AreEqual(deserialize.EntityId, domainEvent.EntityId);
            Assert.AreEqual(deserialize.SecondProp, "testString");
            Assert.AreNotEqual(deserialize.EntityId, Guid.Empty);
        }

        [TestMethod]
        public void TestDeserializationOfIdInInterface_DifferentParameterNameList()
        {
            var domainEvent = new TestEv_DifferentParamName(new Guid("48eb878a-4483-40d9-bf4f-36c85ba5f803"), "testString");
            var serialize =
                "[      {   \"created\":12,       \"version\":14,   \"domainEventType\":\"TestEv_DifferentParamName\",         \"domainEvent\":{    \"secondProp\":\"testString\",           \"entityId\":\"48eb878a-4483-40d9-bf4f-36c85ba5f803\"   }     }   ]";
            var deserialize = new DomainEventWrapperListDeserializer(new JSonHack(), _domainEventFactory).Deserialize(serialize).First().DomainEvent;
            Assert.AreEqual(domainEvent.EntityId, deserialize.EntityId);
            Assert.AreEqual(((TestEv_DifferentParamName)deserialize).SecondProp, "testString");
            Assert.AreNotEqual(deserialize.EntityId, Guid.Empty);
        }

        [TestMethod]
        public void TestDeserializationOfIdInInterface_DifferentParameterNameList_TwoEntries()
        {
            var domainEvent = new TestEv_DifferentParamName(new Guid("84e5447a-0a28-4fe1-af5a-11dd6a43d3dd"), "testString");
            var domainEvent2 = new TestEv_DifferentParamName(new Guid("48eb878a-4483-40d9-bf4f-36c85ba5f803"), "andererString");
            var serialize =
                "[      {   \"created\":12,       \"version\":14, \"domainEventType\":\"TestEv_DifferentParamName\",         \"domainEvent\":{    \"secondProp\":\"testString\",           \"entityId\":\"84e5447a-0a28-4fe1-af5a-11dd6a43d3dd\"   }     },   {   \"created\":12,       \"version\":14,    \"domainEventType\":\"TestEv_DifferentParamName\",        \"domainEvent\":{    \"secondProp\":\"andererString\",           \"entityId\":\"48eb878a-4483-40d9-bf4f-36c85ba5f803\"   }     }   ]";
            var domainEventWrappers = new DomainEventWrapperListDeserializer(new JSonHack(), _domainEventFactory).Deserialize(serialize).ToList();
            var deserialize = domainEventWrappers[0].DomainEvent;
            var deserialize2 = domainEventWrappers[1].DomainEvent;
            Assert.AreEqual(domainEvent.EntityId, deserialize.EntityId);
            Assert.AreEqual(domainEvent2.EntityId, deserialize2.EntityId);
            Assert.AreEqual(((TestEv_DifferentParamName)deserialize).SecondProp, "testString");
            Assert.AreEqual(((TestEv_DifferentParamName)deserialize2).SecondProp, "andererString");
            Assert.AreNotEqual(deserialize.EntityId, Guid.Empty);
            Assert.AreNotEqual(deserialize2.EntityId, Guid.Empty);
        }

        [TestMethod]
        public void TestDeserializationOfIdInInterface_DifferentParameterNameList_TwoEntries_MixedCamelCaseAndPascalCase()
        {
            var domainEvent = new TestEv_DifferentParamName(new Guid("84e5447a-0a28-4fe1-af5a-11dd6a43d3dd"), "testString");
            var domainEvent2 = new TestEv_DifferentParamName(new Guid("48eb878a-4483-40d9-bf4f-36c85ba5f803"), "andererString");
            var serialize =
                "[      {   \"created\":12,       \"version\":14,    \"domainEventType\":\"TestEv_DifferentParamName\",        \"DomainEvent\":{    \"secondProp\":\"testString\",           \"entityId\":\"84e5447a-0a28-4fe1-af5a-11dd6a43d3dd\"   }     },   {   \"created\":12,       \"version\":14,      \"domainEventType\":\"TestEv_DifferentParamName\",      \"domainEvent\":{    \"secondProp\":\"andererString\",           \"EntityId\":\"48eb878a-4483-40d9-bf4f-36c85ba5f803\"   }     }   ]";
            var domainEventWrappers = new DomainEventWrapperListDeserializer(new JSonHack(), _domainEventFactory).Deserialize(serialize).ToList();
            var deserialize = domainEventWrappers[0].DomainEvent;
            var deserialize2 = domainEventWrappers[1].DomainEvent;
            Assert.AreEqual(domainEvent.EntityId, deserialize.EntityId);
            Assert.AreEqual(domainEvent2.EntityId, deserialize2.EntityId);
            Assert.AreEqual(((TestEv_DifferentParamName)deserialize).SecondProp, "testString");
            Assert.AreEqual(((TestEv_DifferentParamName)deserialize2).SecondProp, "andererString");
            Assert.AreNotEqual(deserialize.EntityId, Guid.Empty);
            Assert.AreNotEqual(deserialize2.EntityId, Guid.Empty);
        }

        // This is not supported and might never be
        [TestMethod]
        public void TestDeserializationOfIdInInterface_OwnBackingField()
        {
            var objectConverter = new ObjectConverter();
            var domainEvent = new TestEv_CustomBackingField(Guid.NewGuid());
            var serialize = objectConverter.Serialize(domainEvent);
            var deserialize = objectConverter.Deserialize<IDomainEvent>(serialize);
            Assert.AreEqual(deserialize.EntityId, Guid.Empty);
        }

        [TestMethod]
        public void TestDeserializationOfIdInInterface_GetAutoProperty()
        {
            var objectConverter = new ObjectConverter();
            var domainEvent = new TestEv_AutoProperty(Guid.NewGuid());
            var serialize = objectConverter.Serialize(domainEvent);
            var deserialize = objectConverter.Deserialize<IDomainEvent>(serialize);
            Assert.AreEqual(deserialize.EntityId, new Guid("84e5447a-0a28-4fe1-af5a-11dd6a43d3dd"));
            Assert.AreNotEqual(deserialize.EntityId, Guid.Empty);
        }

        [TestMethod]
        public async Task Entitystream_LoadEventsSince_IdNotDefault()
        {
            var runner = MongoDbRunner.Start("Entitystream_LoadEventsSince_IdNotDefault");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("Entitystream_LoadEventsSince_IdNotDefault");


            var entityStreamRepository =
                new EventRepository(database, new DomainEventDeserializer(new JSonHack()), new ObjectConverter());

            var entityStreamTestEvent = new TestEv(Guid.NewGuid());
            await entityStreamRepository.AppendAsync(new[] {entityStreamTestEvent}, 0);

            var eventsSince = await entityStreamRepository.LoadEvents();

            Assert.AreEqual(entityStreamTestEvent.EntityId, eventsSince.Value.Single().DomainEvent.EntityId);
            Assert.AreNotEqual(entityStreamTestEvent.EntityId, Guid.Empty);

            client.DropDatabase("Entitystream_LoadEventsSince_IdNotDefault");
            runner.Dispose();
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

        public string SecondProp { get; }

        public Guid EntityId { get; }
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