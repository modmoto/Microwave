using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Persistence.MongoDb.Eventstores;
using Microwave.Persistence.UnitTestsSetup.MongoDb;

namespace Microwave.Eventstores.UnitTests
{
    [TestClass]
    public class ReadStoreTests : IntegrationTests
    {
        [TestMethod]
        public async Task Entitystream_LoadEventsSince_IdNotDefault()
        {
            var entityStreamRepository = new EventRepositoryMongoDb(EventMongoDb, new VersionCache(EventMongoDb));

            var entityStreamTestEvent = new TestEv3(Guid.NewGuid());
            await entityStreamRepository.AppendAsync(new[] {entityStreamTestEvent}, 0);

            var eventsSince = await entityStreamRepository.LoadEvents();

            Assert.AreEqual(entityStreamTestEvent.EntityId, eventsSince.Value.Single().DomainEvent.EntityId);
            Assert.AreNotEqual(entityStreamTestEvent.EntityId, Guid.Empty.ToString());
        }
    }

    public class TestEv3 : IDomainEvent
    {
        public TestEv3(Guid entityId)
        {
            EntityId = entityId.ToString();
        }

        public string EntityId { get; }
    }
}