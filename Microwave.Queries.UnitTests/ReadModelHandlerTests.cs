using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application;
using Microwave.Application.Results;
using Microwave.DependencyInjectionExtensions;
using Microwave.Domain;
using Microwave.ObjectPersistences;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class ReadModelHandlerTests
    {
        EventLocationConfig config = new EventLocationConfig(new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json")
            .Build());

        [TestMethod]
        public async Task UpdateReadmodelHandler()
        {
            EntityGuid = Guid.NewGuid();
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("UpdateReadmodelHandler")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries>(queryRepository, new VersionRepository(new
                QueryStorageContext(options)), new FeedMock2(), config);
            await readModelHandler.Update();

            var result = await queryRepository.Load<TestReadModelQuerries>(EntityGuid);
            Assert.AreEqual(EntityGuid, result.Value.Id);
            Assert.AreEqual(14, result.Value.Version);
            Assert.AreEqual("testName", result.Value.ReadModel.Name);
        }

        [Ignore("As handlers are singleton, this should never happen in one service, look that up")]
        [TestMethod]
        public async Task UpdateModelConcurrencyVersionBug()
        {
            EntityGuid = Guid.NewGuid();
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("UpdateModelConcurrencyVersionBug")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries>(queryRepository, new VersionRepository(new
                QueryStorageContext(options)), new FeedMock1(), config);

            var readModelHandler2 = new ReadModelHandler<TestReadModelQuerries>(queryRepository, new VersionRepository(new
                QueryStorageContext(options)), new FeedMock2(), config);


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
                QueryStorageContext(options)), new FeedMock3(), config);

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
                QueryStorageContext(options)), new FeedMock4(), config);

            await readModelHandler.Update();

            var result = await queryRepository.Load<TestReadModelQuerries>(EntityGuid);
            var result2 = await queryRepository.Load<TestReadModelQuerries>(EntityGuid2);
            Assert.AreEqual(EntityGuid, result.Value.Id);
            var condition = result2.Is<NotFound>();
            Assert.IsTrue(condition);
        }

        [TestMethod]
        public async Task UpdateModel_EventsNotAppliedStillUpdatesVersion()
        {
            EntityGuid = Guid.NewGuid();
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("UpdateModelConcurrencyVersionBug")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries_OnlyOneEventAndVersionIsCounted>(queryRepository, new VersionRepository(new
                QueryStorageContext(options)), new FeedMock5(), config);

            await readModelHandler.Update();

            var result = await queryRepository.Load<TestReadModelQuerries_OnlyOneEventAndVersionIsCounted>(EntityGuid);
            Assert.AreEqual(14, result.Value.Version);
            Assert.AreEqual(null, result.Value.ReadModel.Name);
            Assert.AreEqual(EntityGuid, result.Value.ReadModel.Id);
        }

        [TestMethod]
        public async Task UpdateModel_TwoParallelReadModelHandler_SerializationBug()
        {
            EntityGuid = Guid.NewGuid();
            EntityGuid2 = Guid.NewGuid();
            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("UpdateModel_TwoParallelReadModelHandler_SerializationBug")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries_TwoParallelFeeds1>(queryRepository, new VersionRepository(new
                QueryStorageContext(options)), new FeedMock6(), config);

            var readModelHandler2 = new ReadModelHandler<TestReadModelQuerries_TwoParallelFeeds2>(queryRepository, new VersionRepository(new
                QueryStorageContext(options)), new FeedMock7(), config);

            await readModelHandler.Update();
            await readModelHandler2.Update();

            var result = await queryRepository.Load<TestReadModelQuerries_TwoParallelFeeds1>(EntityGuid);
            var result2 = await queryRepository.Load<TestReadModelQuerries_TwoParallelFeeds2>(EntityGuid2);
            Assert.AreEqual(EntityGuid, result.Value.ReadModel.Id);
            Assert.AreEqual(EntityGuid2, result2.Value.ReadModel.Id);
        }

        public static Guid EntityGuid { get; set; }
        public static Guid EntityGuid2 { get; set; }

        public static Task<IEnumerable<DomainEventWrapper>> MakeEvents()
        {
            var wrapper1 = new DomainEventWrapper
            {
                Version = 12,
                DomainEvent = new TestEvnt1(EntityGuid, "testName")
            };

            var wrapper2 = new DomainEventWrapper
            {
                Version = 14,
                DomainEvent = new TestEvnt2(EntityGuid2)
            };
            var list = new List<DomainEventWrapper> {wrapper1, wrapper2};
            return Task.FromResult((IEnumerable<DomainEventWrapper>) list);
        }
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

    public class TestReadModelQuerries_OnlyOneEventAndVersionIsCounted : ReadModel, IHandle<TestEvnt2>
    {
        public void Handle(TestEvnt2 domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public Guid Id { get; set; }
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

    public class FeedMock5 : IEventFeed<ReadModelHandler<TestReadModelQuerries_OnlyOneEventAndVersionIsCounted>>
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

    public class FeedMock6 : IEventFeed<ReadModelHandler<TestReadModelQuerries_TwoParallelFeeds1>>
    {
        public Task<IEnumerable<DomainEventWrapper>> GetEventsAsync(long lastVersion)
        {
            return ReadModelHandlerTests.MakeEvents();
        }
    }

    public class TestReadModelQuerries_TwoParallelFeeds1 : ReadModel, IHandle<TestEvnt1>
    {
        public void Handle(TestEvnt1 domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public Guid Id { get; set; }
    }

    public class FeedMock7 : IEventFeed<ReadModelHandler<TestReadModelQuerries_TwoParallelFeeds2>>
    {
        public Task<IEnumerable<DomainEventWrapper>> GetEventsAsync(long lastVersion)
        {
            return ReadModelHandlerTests.MakeEvents();
        }
    }

    public class TestReadModelQuerries_TwoParallelFeeds2 : ReadModel, IHandle<TestEvnt2>
    {
        public void Handle(TestEvnt2 domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public Guid Id { get; set; }
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