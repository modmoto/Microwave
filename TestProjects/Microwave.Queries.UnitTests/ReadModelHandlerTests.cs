using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.Domain.Results;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Persistence.MongoDb.UnitTestsSetup;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class ReadModelHandlerTests : IntegrationTests
    {
        [TestMethod]
        public async Task UpdateReadmodelHandler()
        {
            EntityGuid = GuidIdentity.Create(Guid.NewGuid());

            var queryRepository = new ReadModelRepository(EventMongoDb);

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries>(queryRepository,
                new VersionRepository(EventMongoDb), new FeedMock2());
            await readModelHandler.Update();

            var result = await queryRepository.Load<TestReadModelQuerries>(EntityGuid);
            Assert.AreEqual(EntityGuid, result.Id);
            Assert.AreEqual(14, result.Version);
            Assert.AreEqual("testName", result.Value.Name);
        }

        [TestMethod]
        public async Task UpdateModel_TwoEntities()
        {
            EntityGuid = GuidIdentity.Create(Guid.NewGuid());
            EntityGuid2 = GuidIdentity.Create(Guid.NewGuid());

            var queryRepository = new ReadModelRepository(EventMongoDb);

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries>(queryRepository,
                new VersionRepository(EventMongoDb), new FeedMock3());

            await readModelHandler.Update();

            var result = await queryRepository.Load<TestReadModelQuerries>(EntityGuid);
            var result2 = await queryRepository.Load<TestReadModelQuerries>(EntityGuid2);
            Assert.AreEqual(EntityGuid, result.Id);
            Assert.AreEqual(EntityGuid2, result2.Id);
        }

        [TestMethod]
        public async Task UpdateModel_EventsPresentThatAreNotHandleble()
        {
            EntityGuid = GuidIdentity.Create(Guid.NewGuid());
            EntityGuid2 = GuidIdentity.Create(Guid.NewGuid());

            var queryRepository = new ReadModelRepository(EventMongoDb);

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries>(queryRepository, new VersionRepository(EventMongoDb), new FeedMock4());

            await readModelHandler.Update();

            var result = await queryRepository.Load<TestReadModelQuerries>(EntityGuid);
            var result2 = await queryRepository.Load<TestReadModelQuerries>(EntityGuid2);
            Assert.AreEqual(EntityGuid, result.Id);
            var condition = result2.Is<NotFound>();
            Assert.IsTrue(condition);
        }

        [TestMethod]
        public async Task UpdateModel_EventsNotAppliedStillUpdatesVersion()
        {
            EntityGuid = GuidIdentity.Create(Guid.NewGuid());

            var queryRepository = new ReadModelRepository(EventMongoDb);

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries_OnlyOneEventAndVersionIsCounted>(
                queryRepository,
                new VersionRepository(EventMongoDb),
                new FeedMock5());

            await readModelHandler.Update();

            var result = await queryRepository.Load<TestReadModelQuerries_OnlyOneEventAndVersionIsCounted>(EntityGuid);
            Assert.AreEqual(14, result.Version);
            Assert.AreEqual(null, result.Value.Name);
            Assert.AreEqual(EntityGuid.Id, result.Value.Id.Id);
        }

        [TestMethod]
        public async Task UpdateModel_TwoParallelReadModelHandler_SerializationBug()
        {
            EntityGuid = GuidIdentity.Create(Guid.NewGuid());
            EntityGuid2 = GuidIdentity.Create(Guid.NewGuid());

            var queryRepository = new ReadModelRepository(EventMongoDb);

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries_TwoParallelFeeds1>(queryRepository, new VersionRepository(EventMongoDb), new FeedMock6());

            var readModelHandler2 = new ReadModelHandler<TestReadModelQuerries_TwoParallelFeeds2>(queryRepository, new VersionRepository(EventMongoDb), new FeedMock7());

            await readModelHandler.Update();
            await readModelHandler2.Update();

            var result = await queryRepository.Load<TestReadModelQuerries_TwoParallelFeeds1>(EntityGuid);
            var result2 = await queryRepository.Load<TestReadModelQuerries_TwoParallelFeeds2>(EntityGuid2);
            Assert.AreEqual(EntityGuid.Id, result.Value.Id.Id);
            Assert.AreEqual(EntityGuid2.Id, result2.Value.IdTotallyDifferenzt.Id);
        }

        [TestMethod]
        public async Task UpdateModel_VersionUpdatedExplicitly()
        {
            EntityGuid = GuidIdentity.Create(Guid.NewGuid());

            var queryRepository = new ReadModelRepository(EventMongoDb);

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries_VerionedHandle>(
                queryRepository,
                new VersionRepository(EventMongoDb),
                new FeedMockVersioned());

            await readModelHandler.Update();

            var result = await queryRepository.Load<TestReadModelQuerries_VerionedHandle>(EntityGuid);
            Assert.AreEqual(EntityGuid.Id, result.Value.EntityId.Id);
            Assert.AreEqual(12, result.Value.Version);
            Assert.AreEqual(14, result.Version);
        }

        public static GuidIdentity EntityGuid { get; set; }
        public static GuidIdentity EntityGuid2 { get; set; }

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

    public class TestReadModelQuerries : ReadModel, IHandle<TestEvnt2>, IHandle<TestEvnt1>
    {
        public void Handle(TestEvnt2 domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public Identity Id { get; set; }
        public void Handle(TestEvnt1 domainEvent)
        {
            Name = domainEvent.Name;
        }

        public string Name { get; set; }
        public override Type GetsCreatedOn => typeof(TestEvnt2);
    }

    public class TestReadModelQuerries_VerionedHandle : ReadModel, IHandleVersioned<TestEvnt2>, IHandle<TestEvnt3>
    {
        public void Handle(TestEvnt2 domainEvent, long version)
        {
            EntityId = domainEvent.EntityId;
            Version = version;
        }

        public Identity EntityId { get; set; }
        public long Version { get; set; }
        public override Type GetsCreatedOn => typeof(TestEvnt2);
        public void Handle(TestEvnt3 domainEvent)
        {
        }
    }

    public class TestReadModelQuerries_OnlyOneEventAndVersionIsCounted : ReadModel, IHandle<TestEvnt2>
    {
        public void Handle(TestEvnt2 domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public Identity Id { get; set; }
        public string Name { get; set; }
        public override Type GetsCreatedOn => typeof(TestEvnt2);
    }

    public class FeedMock2 : IEventFeed<ReadModelHandler<TestReadModelQuerries>>
    {
        public Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset))
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

    public class FeedMock5 : IEventFeed<ReadModelHandler<TestReadModelQuerries_OnlyOneEventAndVersionIsCounted>>
    {
        public Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset))
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

    public class FeedMockVersioned : IEventFeed<ReadModelHandler<TestReadModelQuerries_VerionedHandle>>
    {
        public Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset))
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

    public class FeedMock6 : IEventFeed<ReadModelHandler<TestReadModelQuerries_TwoParallelFeeds1>>
    {
        public Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset))
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

        public Identity Id { get; set; }
        public override Type GetsCreatedOn => typeof(TestEvnt1);
    }

    public class FeedMock7 : IEventFeed<ReadModelHandler<TestReadModelQuerries_TwoParallelFeeds2>>
    {
        public Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset))
        {
            return ReadModelHandlerTests.MakeEvents();
        }
    }

    public class TestReadModelQuerries_TwoParallelFeeds2 : ReadModel, IHandle<TestEvnt2>
    {
        public void Handle(TestEvnt2 domainEvent)
        {
            IdTotallyDifferenzt = domainEvent.EntityId;
        }

        public Identity IdTotallyDifferenzt { get; set; }
        public override Type GetsCreatedOn => typeof(TestEvnt2);
    }

    public class FeedMock3 : IEventFeed<ReadModelHandler<TestReadModelQuerries>>
    {
        public Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset))
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

    public class FeedMock4 : IEventFeed<ReadModelHandler<TestReadModelQuerries>>
    {
        public Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset))
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

    public class FeedMock1 : IEventFeed<ReadModelHandler<TestReadModelQuerries>>
    {
        public Task<IEnumerable<SubscribedDomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset))
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
        public TestEvnt2(GuidIdentity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }

    public class TestEvnt3 : IDomainEvent, ISubscribedDomainEvent
    {
        public TestEvnt3(GuidIdentity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }

    public class TestEvnt1 : IDomainEvent, ISubscribedDomainEvent
    {
        public TestEvnt1(GuidIdentity entityId, string name)
        {
            EntityId = entityId;
            Name = name;
        }

        public Identity EntityId { get; }
        public string Name { get; }
    }
}