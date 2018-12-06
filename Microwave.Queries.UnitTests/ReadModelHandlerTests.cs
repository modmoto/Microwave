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

            var readModelHandler = new ReadModelHandler<TestReadModel, TestEvnt2>(queryRepository, new VersionRepository(new
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

        public Guid Id { get; set; }
    }

    public class FeedMock : IEventFeed<TestEvnt2>
    {
        public Task<IEnumerable<DomainEventHto<TestEvnt2>>> GetEventsAsync(long lastVersion)
        {
            var domainEventWrapper = new DomainEventHto<TestEvnt2>
            {
                Version = 12,
                DomainEvent = new TestEvnt2(ReadModelHandlerTests.EntityGuid)
            };
            var domainEventWrappe2 = new DomainEventHto<TestEvnt2>
            {
                Version = 14,
                DomainEvent = new TestEvnt2(ReadModelHandlerTests.EntityGuid)
            };
            var list = new List<DomainEventHto<TestEvnt2>> {domainEventWrapper, domainEventWrappe2};
            return Task.FromResult((IEnumerable<DomainEventHto<TestEvnt2>>) list);
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