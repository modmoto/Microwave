using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application;
using Microwave.Application.Ports;
using Microwave.Domain;
using Microwave.EventStores;
using Microwave.ObjectPersistences;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class ReadModelHandlerTests
    {
        [TestMethod]
        public async Task UpdateReadmodelHandler()
        {
            EntityGuid = Guid.NewGuid();
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("UpdateReadmodelHandler")
                .Options;

            var optionsStore = new DbContextOptionsBuilder<EventStoreContext>()
                .UseInMemoryDatabase("UpdateReadmodelHandlerEventStore")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());

            var readModelHandler = new ReadModelHandler<TestReadModel>(queryRepository, new VersionRepository(new
                EventStoreContext(optionsStore)), new FeedMock());
            await readModelHandler.Update();

            var result = await queryRepository.Load<TestReadModel>(EntityGuid);
            Assert.AreEqual(EntityGuid, result.Value.Id);
            Assert.AreEqual(14, result.Value.Version);
        }

        public static Guid EntityGuid { get; set; }
    }

    public class TestReadModel : ReadModel, IHandle<TestEvnt2>
    {
        public void Handle(TestEvnt2 domainEvent)
        {
            Id = domainEvent.EntityId;
        }
    }

    public class FeedMock : IEventFeed
    {
        public Task<IEnumerable<DomainEventWrapper>> GetEvents(long lastVersion)
        {
            var domainEventWrapper = new DomainEventWrapper
            {
                Version = 12,
                DomainEvent = new TestEvnt2(ReadModelHandlerTests.EntityGuid)
            };
            var domainEventWrappe2 = new DomainEventWrapper
            {
                Version = 14,
                DomainEvent = new TestEvnt2(ReadModelHandlerTests.EntityGuid)
            };
            var list = new List<DomainEventWrapper> {domainEventWrapper, domainEventWrappe2};
            return Task.FromResult((IEnumerable<DomainEventWrapper>) list);
        }
    }

    public class TestEvnt2 : IDomainEvent
    {
        public TestEvnt2(Guid entityId)
        {
            EntityId = entityId;
        }

        public Guid EntityId { get; }
    }
}