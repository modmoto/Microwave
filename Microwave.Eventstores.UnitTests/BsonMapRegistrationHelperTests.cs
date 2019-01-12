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