using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain;
using Microwave.EventStores;
using Microwave.EventStores.Ports;
using Microwave.Persistence.MongoDb.EventStores;
using Microwave.Persistence.MongoDb.UnitTests.Eventstores;
using Moq;

namespace Microwave.Eventstores.UnitTests.DomainEvents
{
    [TestClass]
    public class BsonMapRegistrationHelperTestsWithGoodEvents : IntegrationTests
    {
        [TestMethod]
        public async Task AddEvents_ForeachMethod()
        {
            BsonMapRegistrationHelpers.AddBsonMapsForMicrowave(typeof(TestEventAllOk).Assembly);

            var eventRepository = new EventRepository(EventDatabase, new VersionCache(EventDatabase));
            var snapShotRepo = new Mock<ISnapShotRepository>();
            snapShotRepo.Setup(re => re.LoadSnapShot<TestEntity>(It.IsAny<Identity>()))
                .ReturnsAsync(new DefaultSnapshot<TestEntity>());

            var eventStore = new EventStore(eventRepository, snapShotRepo.Object);

            var newGuid = Guid.NewGuid();

            await eventStore.AppendAsync(
                new List<IDomainEvent> {new TestEventAllOk(GuidIdentity.Create(newGuid), "Simon")},
                0);

            var result = await eventRepository.LoadEvents();

            Assert.AreEqual(1, result.Value.Count());
            Assert.AreEqual(newGuid.ToString(), result.Value.Single().DomainEvent.EntityId.Id);
            Assert.AreEqual("Simon", ((TestEventAllOk) result.Value.Single().DomainEvent).Name);
        }
    }

    public class TestEntity
    {
    }

    public class TestEventAllOk : IDomainEvent
    {
        public TestEventAllOk(GuidIdentity entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }

        public Identity EntityId { get; }
        public string Name { get; }
    }
}