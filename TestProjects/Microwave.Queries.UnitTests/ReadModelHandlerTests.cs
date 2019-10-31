using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Results;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Persistence.UnitTestsSetup.MongoDb;
using Microwave.Queries.Handler;
using Microwave.Queries.Ports;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class ReadModelHandlerTests : IntegrationTests
    {
        [TestMethod]
        public async Task UpdateReadmodelHandler()
        {
            EntityGuid = Guid.NewGuid();

            var queryRepository = new ReadModelRepositoryMongoDb(EventMongoDb);

            var readModelHandler = new ReadModelEventHandler<TestReadModelQuerries>(queryRepository,
                new VersionRepositoryMongoDb(EventMongoDb), new FeedMock2());
            await readModelHandler.Update();

            var result = await queryRepository.LoadAsync<TestReadModelQuerries>(EntityGuid.ToString());
            Assert.AreEqual(EntityGuid.ToString(), result.Value.Id);
            Assert.AreEqual(14, result.Value.Version);
            Assert.AreEqual("testName", result.Value.Name);
        }

        [TestMethod]
        public async Task UpdateModel_TwoEntities()
        {
            EntityGuid = Guid.NewGuid();
            EntityGuid2 = Guid.NewGuid();

            var queryRepository = new ReadModelRepositoryMongoDb(EventMongoDb);

            var readModelHandler = new ReadModelEventHandler<TestReadModelQuerries>(queryRepository,
                new VersionRepositoryMongoDb(EventMongoDb), new FeedMock3());

            await readModelHandler.Update();

            var result = await queryRepository.LoadAsync<TestReadModelQuerries>(EntityGuid.ToString());
            var result2 = await queryRepository.LoadAsync<TestReadModelQuerries>(EntityGuid2.ToString());
            Assert.AreEqual(EntityGuid.ToString(), result.Value.Id);
            Assert.AreEqual(EntityGuid2.ToString(), result2.Value.Id);
        }

        [TestMethod]
        public async Task UpdateModel_EventsPresentThatAreNotHandleble()
        {
            EntityGuid = Guid.NewGuid();
            EntityGuid2 = Guid.NewGuid();

            var queryRepository = new ReadModelRepositoryMongoDb(EventMongoDb);

            var readModelHandler = new ReadModelEventHandler<TestReadModelQuerries>(queryRepository, new VersionRepositoryMongoDb(EventMongoDb), new FeedMock4());

            await readModelHandler.Update();

            var result = await queryRepository.LoadAsync<TestReadModelQuerries>(EntityGuid.ToString());
            var result2 = await queryRepository.LoadAsync<TestReadModelQuerries>(EntityGuid2.ToString());
            Assert.AreEqual(EntityGuid.ToString(), result.Value.Id);
            var condition = result2.Is<NotFound>();
            Assert.IsTrue(condition);
        }

        [TestMethod]
        public async Task UpdateModel_EventsNotAppliedStillUpdatesVersion()
        {
            EntityGuid = Guid.NewGuid();

            var queryRepository = new ReadModelRepositoryMongoDb(EventMongoDb);

            var readModelHandler = new ReadModelEventHandler<TestReadModelQuerries_OnlyOneEventAndVersionIsCounted>(
                queryRepository,
                new VersionRepositoryMongoDb(EventMongoDb),
                new FeedMock5());

            await readModelHandler.Update();

            var result = await queryRepository.LoadAsync<TestReadModelQuerries_OnlyOneEventAndVersionIsCounted>
            (EntityGuid.ToString());
            Assert.AreEqual(14, result.Value.Version);
            Assert.AreEqual(null, result.Value.Name);
            Assert.AreEqual(EntityGuid.ToString(), result.Value.Id);
        }

        [TestMethod]
        public async Task UpdateModel_TwoParallelReadModelHandler_SerializationBug()
        {
            EntityGuid = Guid.NewGuid();
            EntityGuid2 = Guid.NewGuid();

            var queryRepository = new ReadModelRepositoryMongoDb(EventMongoDb);

            var readModelHandler = new ReadModelEventHandler<TestReadModelQuerries_TwoParallelFeeds1>(queryRepository, new VersionRepositoryMongoDb(EventMongoDb), new FeedMock6());

            var readModelHandler2 = new ReadModelEventHandler<TestReadModelQuerries_TwoParallelFeeds2>(queryRepository, new VersionRepositoryMongoDb(EventMongoDb), new FeedMock7());

            await readModelHandler.Update();
            await readModelHandler2.Update();

            var result = await queryRepository.LoadAsync<TestReadModelQuerries_TwoParallelFeeds1>(EntityGuid.ToString());
            var result2 = await queryRepository.LoadAsync<TestReadModelQuerries_TwoParallelFeeds2>(EntityGuid2.ToString());
            Assert.AreEqual(EntityGuid.ToString(), result.Value.Id);
            Assert.AreEqual(EntityGuid2.ToString(), result2.Value.IdTotallyDifferenzt);
        }

        [TestMethod]
        public async Task UpdateModel_VersionUpdatedExplicitly()
        {
            EntityGuid = Guid.NewGuid();

            var queryRepository = new ReadModelRepositoryMongoDb(EventMongoDb);

            var readModelHandler = new ReadModelEventHandler<TestReadModelQuerries_VerionedHandle>(
                queryRepository,
                new VersionRepositoryMongoDb(EventMongoDb),
                new FeedMockVersioned());

            await readModelHandler.Update();

            var result = await queryRepository.LoadAsync<TestReadModelQuerries_VerionedHandle>(EntityGuid.ToString());
            Assert.AreEqual(EntityGuid.ToString(), result.Value.EntityId);
            Assert.AreEqual(12, result.Value.InnerVersion);
            Assert.AreEqual(14, result.Value.Version);
        }

        public static Guid EntityGuid { get; set; }
        public static Guid EntityGuid2 { get; set; }

        public static Task<IEnumerable<SubscribedDomainEventWrapper>> MakeEvents()
        {
            var wrapper1 = new SubscribedDomainEventWrapper
            {
                Version = 12,
                DomainEvent = new TestEvnt1(EntityGuid, "testName")
            };

            var wrapper2 = new SubscribedDomainEventWrapper
            {
                Version = 14,
                DomainEvent = new TestEvnt2(EntityGuid2)
            };
            var list = new List<SubscribedDomainEventWrapper> {wrapper1, wrapper2};
            return Task.FromResult((IEnumerable<SubscribedDomainEventWrapper>) list);
        }
    }

    public class TestReadModelQuerries : ReadModel<TestEvnt2>, IHandle<TestEvnt2>, IHandle<TestEvnt1>
    {
        public void Handle(TestEvnt2 domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public string Id { get; set; }
        public void Handle(TestEvnt1 domainEvent)
        {
            Name = domainEvent.Name;
        }

        public string Name { get; set; }
    }

    public class TestReadModelQuerries_VerionedHandle : ReadModel<TestEvnt2>, IHandleVersioned<TestEvnt2>, IHandle<TestEvnt3>
    {
        public void Handle(TestEvnt2 domainEvent, long version)
        {
            EntityId = domainEvent.EntityId;
            InnerVersion = version;
        }

        public string EntityId { get; set; }
        public long InnerVersion { get; set; }
        public void Handle(TestEvnt3 domainEvent)
        {
        }
    }

    public class TestReadModelQuerries_OnlyOneEventAndVersionIsCounted : ReadModel<TestEvnt2>, IHandle<TestEvnt2>
    {
        public void Handle(TestEvnt2 domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public string Id { get; set; }
        public string Name { get; set; }
    }

    public class FeedMock2 : IEventFeed<ReadModelEventHandler<TestReadModelQuerries>>
    {
        public Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(long lastVersion = 0)
        {
            var domainEventWrapper = new SubscribedDomainEventWrapper
            {
                Version = 12,
                DomainEvent = new TestEvnt2(ReadModelHandlerTests.EntityGuid)
            };
            var domainEventWrappe2 = new SubscribedDomainEventWrapper
            {
                Version = 14,
                DomainEvent = new TestEvnt1(ReadModelHandlerTests.EntityGuid, "testName")
            };
            var list = new List<SubscribedDomainEventWrapper> {domainEventWrapper, domainEventWrappe2};
            return Task.FromResult((IEnumerable<SubscribedDomainEventWrapper>) list);
        }
    }

    public class FeedMock5 : IEventFeed<ReadModelEventHandler<TestReadModelQuerries_OnlyOneEventAndVersionIsCounted>>
    {
        public Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(long lastVersion = 0)
        {
            var domainEventWrapper = new SubscribedDomainEventWrapper
            {
                Version = 12,
                DomainEvent = new TestEvnt2(ReadModelHandlerTests.EntityGuid)
            };
            var domainEventWrappe2 = new SubscribedDomainEventWrapper
            {
                Version = 14,
                DomainEvent = new TestEvnt1(ReadModelHandlerTests.EntityGuid, "testName")
            };
            var list = new List<SubscribedDomainEventWrapper> {domainEventWrapper, domainEventWrappe2};
            return Task.FromResult((IEnumerable<SubscribedDomainEventWrapper>) list);
        }
    }

    public class FeedMockVersioned : IEventFeed<ReadModelEventHandler<TestReadModelQuerries_VerionedHandle>>
    {
        public Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(long lastVersion = 0)
        {
            var domainEventWrapper = new SubscribedDomainEventWrapper
            {
                Version = 12,
                DomainEvent = new TestEvnt2(ReadModelHandlerTests.EntityGuid)
            };
            var domainEventWrappe2 = new SubscribedDomainEventWrapper
            {
                Version = 14,
                DomainEvent = new TestEvnt3(ReadModelHandlerTests.EntityGuid)
            };
            var list = new List<SubscribedDomainEventWrapper> {domainEventWrapper, domainEventWrappe2};
            return Task.FromResult((IEnumerable<SubscribedDomainEventWrapper>) list);
        }
    }

    public class FeedMock6 : IEventFeed<ReadModelEventHandler<TestReadModelQuerries_TwoParallelFeeds1>>
    {
        public Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(long lastVersion = 0)
        {
            return ReadModelHandlerTests.MakeEvents();
        }
    }

    public class TestReadModelQuerries_TwoParallelFeeds1 : ReadModel<TestEvnt1>, IHandle<TestEvnt1>
    {
        public void Handle(TestEvnt1 domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public string Id { get; set; }
    }

    public class FeedMock7 : IEventFeed<ReadModelEventHandler<TestReadModelQuerries_TwoParallelFeeds2>>
    {
        public Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(long lastVersion = 0)
        {
            return ReadModelHandlerTests.MakeEvents();
        }
    }

    public class TestReadModelQuerries_TwoParallelFeeds2 : ReadModel<TestEvnt2>, IHandle<TestEvnt2>
    {
        public void Handle(TestEvnt2 domainEvent)
        {
            IdTotallyDifferenzt = domainEvent.EntityId;
        }

        public string IdTotallyDifferenzt { get; set; }
    }

    public class FeedMock3 : IEventFeed<ReadModelEventHandler<TestReadModelQuerries>>
    {
        public Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(long lastVersion = 0)
        {
            var domainEventWrapper = new SubscribedDomainEventWrapper
            {
                Version = 12,
                DomainEvent = new TestEvnt2(ReadModelHandlerTests.EntityGuid)
            };
            var domainEventWrapper2 = new SubscribedDomainEventWrapper
            {
                Version = 12,
                DomainEvent = new TestEvnt2(ReadModelHandlerTests.EntityGuid2)
            };
            var list = new List<SubscribedDomainEventWrapper> {domainEventWrapper, domainEventWrapper2};
            return Task.FromResult((IEnumerable<SubscribedDomainEventWrapper>) list);
        }
    }

    public class FeedMock4 : IEventFeed<ReadModelEventHandler<TestReadModelQuerries>>
    {
        public Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(long lastVersion = 0)
        {
            var domainEventWrapper = new SubscribedDomainEventWrapper
            {
                Version = 12,
                DomainEvent = new TestEvnt2(ReadModelHandlerTests.EntityGuid)
            };
            var domainEventWrapper2 = new SubscribedDomainEventWrapper
            {
                Version = 12,
                DomainEvent = new TestEvnt3(ReadModelHandlerTests.EntityGuid2)
            };
            var list = new List<SubscribedDomainEventWrapper> {domainEventWrapper, domainEventWrapper2};
            return Task.FromResult((IEnumerable<SubscribedDomainEventWrapper>) list);
        }
    }

    public class FeedMock1 : IEventFeed<ReadModelEventHandler<TestReadModelQuerries>>
    {
        public Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(long lastVersion = 0)
        {
            var domainEventWrapper = new SubscribedDomainEventWrapper
            {
                Version = 15,
                DomainEvent = new TestEvnt1(ReadModelHandlerTests.EntityGuid, "ersterName")
            };
            var domainEventWrappe2 = new SubscribedDomainEventWrapper
            {
                Version = 17,
                DomainEvent = new TestEvnt1(ReadModelHandlerTests.EntityGuid, "zweiterName")
            };
            var list = new List<SubscribedDomainEventWrapper> {domainEventWrapper, domainEventWrappe2};
            return Task.FromResult((IEnumerable<SubscribedDomainEventWrapper>) list);
        }
    }

    public class TestEvnt2 : IDomainEvent, ISubscribedDomainEvent
    {
        public TestEvnt2(Guid entityId)
        {
            EntityId = entityId.ToString();
        }

        public string EntityId { get; }
    }

    public class TestEvnt3 : IDomainEvent, ISubscribedDomainEvent
    {
        public TestEvnt3(Guid entityId)
        {
            EntityId = entityId.ToString();
        }

        public string EntityId { get; }
    }

    public class TestEvnt1 : IDomainEvent, ISubscribedDomainEvent
    {
        public TestEvnt1(Guid entityId, string name)
        {
            EntityId = entityId.ToString();
            Name = name;
        }

        public string EntityId { get; }
        public string Name { get; }
    }
}