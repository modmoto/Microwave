using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application;
using Microwave.Application.Results;
using Microwave.Domain;
using Microwave.Eventstores.UnitTests;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class ReadModelHandlerTests : IntegrationTests
    {
        [TestMethod]
        public async Task UpdateReadmodelHandler()
        {
            EntityGuid = GuidIdentity.Create(Guid.NewGuid());

            var queryRepository = new ReadModelRepository(ReadModelDatabase);

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries>(queryRepository,
                new VersionRepository(ReadModelDatabase), new FeedMock2());
            await readModelHandler.Update();

            var result = await queryRepository.Load<TestReadModelQuerries>(EntityGuid);
            Assert.AreEqual(EntityGuid, result.Id);
            Assert.AreEqual("testName", result.Value.Name);
        }

        [TestMethod]
        public async Task UpdateModel_TwoEntities()
        {
            EntityGuid = GuidIdentity.Create(Guid.NewGuid());
            EntityGuid2 = GuidIdentity.Create(Guid.NewGuid());

            var queryRepository = new ReadModelRepository(ReadModelDatabase);

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries>(queryRepository,
                new VersionRepository(ReadModelDatabase), new FeedMock3());

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

            var queryRepository = new ReadModelRepository(ReadModelDatabase);

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries>(queryRepository, new VersionRepository(ReadModelDatabase), new FeedMock4());

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

            var queryRepository = new ReadModelRepository(ReadModelDatabase);

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries_OnlyOneEventAndVersionIsCounted>(
                queryRepository,
                new VersionRepository(ReadModelDatabase),
                new FeedMock5());

            await readModelHandler.Update();

            var result = await queryRepository.Load<TestReadModelQuerries_OnlyOneEventAndVersionIsCounted>(EntityGuid);
            Assert.AreEqual(null, result.Value.Name);
            Assert.AreEqual(EntityGuid.Id, result.Value.Id.Id);
        }

        [TestMethod]
        public async Task UpdateModel_TwoParallelReadModelHandler_SerializationBug()
        {
            EntityGuid = GuidIdentity.Create(Guid.NewGuid());
            EntityGuid2 = GuidIdentity.Create(Guid.NewGuid());

            var queryRepository = new ReadModelRepository(ReadModelDatabase);

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries_TwoParallelFeeds1>(queryRepository, new VersionRepository(ReadModelDatabase), new FeedMock6());

            var readModelHandler2 = new ReadModelHandler<TestReadModelQuerries_TwoParallelFeeds2>(queryRepository, new VersionRepository(ReadModelDatabase), new FeedMock7());

            await readModelHandler.Update();
            await readModelHandler2.Update();

            var result = await queryRepository.Load<TestReadModelQuerries_TwoParallelFeeds1>(EntityGuid);
            var result2 = await queryRepository.Load<TestReadModelQuerries_TwoParallelFeeds2>(EntityGuid2);
            Assert.AreEqual(EntityGuid.Id, result.Value.Id.Id);
            Assert.AreEqual(EntityGuid2.Id, result2.Value.IdTotallyDifferenzt.Id);
        }

        public static GuidIdentity EntityGuid { get; set; }
        public static GuidIdentity EntityGuid2 { get; set; }

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

    public class TestReadModelQuerries : ReadModel, IHandle<TestEvnt2>, IHandleVersioned<TestEvnt1>
    {
        public void Handle(TestEvnt2 domainEvent)
        {
            Id = domainEvent.EntityId;
        }

        public Identity Id { get; set; }
        public void Handle(TestEvnt1 domainEvent, long version)
        {
            Name = domainEvent.Name;
        }

        public string Name { get; set; }
        public override Type GetsCreatedOn => typeof(TestEvnt2);
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
        public Task<IEnumerable<DomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset))
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
        public Task<IEnumerable<DomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset))
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
        public Task<IEnumerable<DomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset))
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
        public Task<IEnumerable<DomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset))
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
        public Task<IEnumerable<DomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset))
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
        public Task<IEnumerable<DomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset))
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
        public Task<IEnumerable<DomainEventWrapper>> GetEventsAsync(DateTimeOffset since = default(DateTimeOffset))
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
        public TestEvnt2(GuidIdentity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }

    public class TestEvnt3 : IDomainEvent
    {
        public TestEvnt3(GuidIdentity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }

    public class TestEvnt1 : IDomainEvent
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