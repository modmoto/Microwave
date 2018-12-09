using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application;
using Microwave.Application.Results;
using Microwave.Domain;
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

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries>(queryRepository, new VersionRepository(new
                QueryStorageContext(options)), new FeedMock2());
            await readModelHandler.Update();

            var result = await queryRepository.Load<TestReadModelQuerries>(EntityGuid);
            Assert.AreEqual(EntityGuid, result.Value.Id);
            Assert.AreEqual(14, result.Value.Version);
            Assert.AreEqual("testName", result.Value.ReadModel.Name);
        }

        [TestMethod]
        public async Task UpdateModelConcurrencyVersionBug()
        {
            EntityGuid = Guid.NewGuid();
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("UpdateModelConcurrencyVersionBug")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries>(queryRepository, new VersionRepository(new
                QueryStorageContext(options)), new FeedMock1());

            var readModelHandler2 = new ReadModelHandler<TestReadModelQuerries>(queryRepository, new VersionRepository(new
                QueryStorageContext(options)), new FeedMock2());


            var update = readModelHandler.Update();
            var update2 = readModelHandler2.Update();

            await Task.WhenAll(update2, update);

            var result = await queryRepository.Load<TestReadModelQuerries>(EntityGuid);
            Assert.AreEqual(EntityGuid, result.Value.Id);
            Assert.AreEqual(17, result.Value.Version);
            Assert.AreEqual("testName", result.Value.ReadModel.Name);
            Assert.AreEqual(EntityGuid, result.Value.ReadModel.Id);
        }

        [TestMethod]
        public async Task UpdateModel_TwoEntities()
        {
            EntityGuid = Guid.NewGuid();
            EntityGuid2 = Guid.NewGuid();
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("UpdateModelConcurrencyVersionBug")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries>(queryRepository, new VersionRepository(new
                QueryStorageContext(options)), new FeedMock3());

            await readModelHandler.Update();

            var result = await queryRepository.Load<TestReadModelQuerries>(EntityGuid);
            var result2 = await queryRepository.Load<TestReadModelQuerries>(EntityGuid2);
            Assert.AreEqual(EntityGuid, result.Value.Id);
            Assert.AreEqual(EntityGuid2, result2.Value.Id);
        }

        [TestMethod]
        public async Task UpdateModel_EventsPresentThatAreNotHandleble()
        {
            EntityGuid = Guid.NewGuid();
            EntityGuid2 = Guid.NewGuid();
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("UpdateModelConcurrencyVersionBug")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries>(queryRepository, new VersionRepository(new
                QueryStorageContext(options)), new FeedMock4());

            await readModelHandler.Update();

            var result = await queryRepository.Load<TestReadModelQuerries>(EntityGuid);
            var result2 = await queryRepository.Load<TestReadModelQuerries>(EntityGuid2);
            Assert.AreEqual(EntityGuid, result.Value.Id);
            var condition = result2.Is<NotFound>();
            Assert.IsTrue(condition);
        }

        public static Guid EntityGuid { get; set; }
        public static Guid EntityGuid2 { get; set; }
    }

    public class TestReadModelQuerries : ReadModel, IHandle<TestEvnt2>, IHandle<TestEvnt1>
    {
        public void Handle(TestEvnt2 domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public Guid Id { get; set; }
        public void Handle(TestEvnt1 domainEvent)
        {
            Name = domainEvent.Name;
        }

        public string Name { get; set; }
    }

    public class FeedMock2 : IEventFeed<ReadModelHandler<TestReadModelQuerries>>
    {
        public Task<IEnumerable<DomainEventWrapper>> GetEventsAsync(long lastVersion)
        {
            var domainEventWrapper = new DomainEventWrapper
            {
                Version = 12,
                DomainEvent = new TestEvnt2(ReadModelHandlerTests.EntityGuid)
            };
            var domainEventWrappe2 = new DomainEventWrapper
            {
                Version = 14,
                DomainEvent = new TestEvnt1(ReadModelHandlerTests.EntityGuid, "testName")
            };
            var list = new List<DomainEventWrapper> {domainEventWrapper, domainEventWrappe2};
            return Task.FromResult((IEnumerable<DomainEventWrapper>) list);
        }
    }

    public class FeedMock3 : IEventFeed<ReadModelHandler<TestReadModelQuerries>>
    {
        public Task<IEnumerable<DomainEventWrapper>> GetEventsAsync(long lastVersion)
        {
            var domainEventWrapper = new DomainEventWrapper
            {
                Version = 12,
                DomainEvent = new TestEvnt2(ReadModelHandlerTests.EntityGuid)
            };
            var domainEventWrapper2 = new DomainEventWrapper
            {
                Version = 12,
                DomainEvent = new TestEvnt2(ReadModelHandlerTests.EntityGuid2)
            };
            var list = new List<DomainEventWrapper> {domainEventWrapper, domainEventWrapper2};
            return Task.FromResult((IEnumerable<DomainEventWrapper>) list);
        }
    }

    public class FeedMock4 : IEventFeed<ReadModelHandler<TestReadModelQuerries>>
    {
        public Task<IEnumerable<DomainEventWrapper>> GetEventsAsync(long lastVersion)
        {
            var domainEventWrapper = new DomainEventWrapper
            {
                Version = 12,
                DomainEvent = new TestEvnt2(ReadModelHandlerTests.EntityGuid)
            };
            var domainEventWrapper2 = new DomainEventWrapper
            {
                Version = 12,
                DomainEvent = new TestEvnt3(ReadModelHandlerTests.EntityGuid2)
            };
            var list = new List<DomainEventWrapper> {domainEventWrapper, domainEventWrapper2};
            return Task.FromResult((IEnumerable<DomainEventWrapper>) list);
        }
    }

    public class FeedMock1 : IEventFeed<ReadModelHandler<TestReadModelQuerries>>
    {
        public Task<IEnumerable<DomainEventWrapper>> GetEventsAsync(long lastVersion)
        {
            var domainEventWrapper = new DomainEventWrapper
            {
                Version = 15,
                DomainEvent = new TestEvnt1(ReadModelHandlerTests.EntityGuid, "ersterName")
            };
            var domainEventWrappe2 = new DomainEventWrapper
            {
                Version = 17,
                DomainEvent = new TestEvnt1(ReadModelHandlerTests.EntityGuid, "zweiterName")
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

    public class TestEvnt3 : IDomainEvent
    {
        public TestEvnt3(Guid entityId)
        {
            EntityId = entityId;
        }

        public Guid EntityId { get; }
    }

    public class TestEvnt1 : IDomainEvent
    {
        public TestEvnt1(Guid entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }

        public Guid EntityId { get; }
        public string Name { get; }
    }
}