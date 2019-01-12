using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Moq;

namespace Microwave.Eventstores.UnitTests
{
    [TestClass]
    public class BsonMapRegistrationHelperTests : IntegrationTests
    {
        [TestMethod]
        public async Task AddEvents_ConstructorBsonBug()
        {
            BsonMapRegistrationHelpers.AddBsonMapFor<TestEvent_BsonBug>();

            var eventRepository = new EventRepository(EventDatabase, new VersionCache(EventDatabase));
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<Identity>()))
                .ReturnsAsync(new DefaultSnapshot<TestEntity>());

            var eventStore = new EventStore(eventRepository, snapShotRepo.Object);

            var newGuid = Guid.NewGuid();

            await eventStore.AppendAsync(new List<IDomainEvent> { new TestEvent_BsonBug(GuidIdentity.Create(newGuid), "Simon")},
                0);

            var result = await eventRepository.LoadEvents();

            Assert.AreEqual(1, result.Value.Count());
            Assert.AreEqual(newGuid.ToString(), result.Value.Single().DomainEvent.EntityId.Id);
            Assert.AreEqual("Simon", ((TestEvent_BsonBug)result.Value.Single().DomainEvent).Name);
        }

        [TestMethod]
        public async Task AddEvents_ConstructorBson_UnconventionalOderring()
        {
            BsonMapRegistrationHelpers.AddBsonMapFor<TestEvent_UnconventionalOderring>();

            var eventRepository = new EventRepository(EventDatabase, new VersionCache(EventDatabase));
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<Identity>()))
                .ReturnsAsync(new DefaultSnapshot<TestEntity>());

            var eventStore = new EventStore(eventRepository, snapShotRepo.Object);

            await eventStore.AppendAsync(new List<IDomainEvent> { new TestEvent_UnconventionalOderring("Simon", StringIdentity.Create("whatever"))},
                0);

            var result = await eventRepository.LoadEvents();

            Assert.AreEqual(1, result.Value.Count());
            Assert.AreEqual("whatever", result.Value.Single().DomainEvent.EntityId.Id);
            Assert.AreEqual("Simon", ((TestEvent_UnconventionalOderring)result.Value.Single().DomainEvent).Name);
        }
    }

    public class TestEvent_UnconventionalOderring : IDomainEvent
    {
        public Identity EntityId { get; }
        public string Name { get; }

        public TestEvent_UnconventionalOderring(string name, StringIdentity identity)
        {
            EntityId = identity;
            Name = name;
        }
    }

    public class TestEvent_BsonBug : IDomainEvent
    {
        public string Name { get; }

        public TestEvent_BsonBug(GuidIdentity entityId, string name)
        {
            Name = name;
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }
}