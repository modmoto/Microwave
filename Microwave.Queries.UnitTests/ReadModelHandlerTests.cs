using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application;
using Microwave.Application.Results;
using Microwave.Domain;
using Microwave.ObjectPersistences;
using Mongo2Go;
using MongoDB.Driver;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class ReadModelHandlerTests
    {
        [TestMethod]
        public async Task UpdateReadmodelHandler()
        {
            var runner = MongoDbRunner.Start("UpdateReadmodelHandler");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("UpdateReadmodelHandler");

            EntityGuid = Guid.NewGuid();

            var queryRepository = new ReadModelRepository(database, new ObjectConverter());

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries>(queryRepository,
                new VersionRepository(database), new FeedMock2());
            await readModelHandler.Update();

            var result = await queryRepository.Load<TestReadModelQuerries>(EntityGuid);
            Assert.AreEqual(EntityGuid, result.Value.Id);
            Assert.AreEqual(14, result.Value.Version);
            Assert.AreEqual("testName", result.Value.ReadModel.Name);
        }

        [TestMethod]
        public async Task UpdateModel_TwoEntities()
        {
            var runner = MongoDbRunner.Start("UpdateModel_TwoEntities");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("UpdateModel_TwoEntities");

            EntityGuid = Guid.NewGuid();
            EntityGuid2 = Guid.NewGuid();

            var queryRepository = new ReadModelRepository(database, new ObjectConverter());

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries>(queryRepository,
                new VersionRepository(database), new FeedMock3());

            await readModelHandler.Update();

            var result = await queryRepository.Load<TestReadModelQuerries>(EntityGuid);
            var result2 = await queryRepository.Load<TestReadModelQuerries>(EntityGuid2);
            Assert.AreEqual(EntityGuid, result.Value.Id);
            Assert.AreEqual(EntityGuid2, result2.Value.Id);
            runner.Dispose();
        }

        [TestMethod]
        public async Task UpdateModel_EventsPresentThatAreNotHandleble()
        {
            var runner = MongoDbRunner.Start("UpdateModel_EventsPresentThatAreNotHandleble");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("UpdateModel_EventsPresentThatAreNotHandleble");

            EntityGuid = Guid.NewGuid();
            EntityGuid2 = Guid.NewGuid();

            var queryRepository = new ReadModelRepository(database, new ObjectConverter());

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries>(queryRepository, new VersionRepository(database), new FeedMock4());

            await readModelHandler.Update();

            var result = await queryRepository.Load<TestReadModelQuerries>(EntityGuid);
            var result2 = await queryRepository.Load<TestReadModelQuerries>(EntityGuid2);
            Assert.AreEqual(EntityGuid, result.Value.Id);
            var condition = result2.Is<NotFound>();
            Assert.IsTrue(condition);
            runner.Dispose();
        }

        [TestMethod]
        public async Task UpdateModel_EventsNotAppliedStillUpdatesVersion()
        {
            var runner = MongoDbRunner.Start("LoadAllReadModels");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("LoadAllReadModels");

            EntityGuid = Guid.NewGuid();

            var queryRepository = new ReadModelRepository(database, new ObjectConverter());

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries_OnlyOneEventAndVersionIsCounted>(
                queryRepository,
                new VersionRepository(database),
                new FeedMock5());

            await readModelHandler.Update();

            var result = await queryRepository.Load<TestReadModelQuerries_OnlyOneEventAndVersionIsCounted>(EntityGuid);
            Assert.AreEqual(14, result.Value.Version);
            Assert.AreEqual(null, result.Value.ReadModel.Name);
            Assert.AreEqual(EntityGuid, result.Value.ReadModel.Id);
            runner.Dispose();
        }

        [TestMethod]
        public async Task UpdateModel_TwoParallelReadModelHandler_SerializationBug()
        {
            var runner = MongoDbRunner.Start("LoadAllReadModels");
            var client = new MongoClient(runner.ConnectionString);
            var database = client.GetDatabase("LoadAllReadModels");

            EntityGuid = Guid.NewGuid();
            EntityGuid2 = Guid.NewGuid();

            var queryRepository = new ReadModelRepository(database, new ObjectConverter());

            var readModelHandler = new ReadModelHandler<TestReadModelQuerries_TwoParallelFeeds1>(queryRepository, new VersionRepository(database), new FeedMock6());

            var readModelHandler2 = new ReadModelHandler<TestReadModelQuerries_TwoParallelFeeds2>(queryRepository, new VersionRepository(database), new FeedMock7());

            await readModelHandler.Update();
            await readModelHandler2.Update();

            var result = await queryRepository.Load<TestReadModelQuerries_TwoParallelFeeds1>(EntityGuid);
            var result2 = await queryRepository.Load<TestReadModelQuerries_TwoParallelFeeds2>(EntityGuid2);
            Assert.AreEqual(EntityGuid, result.Value.ReadModel.Id);
            Assert.AreEqual(EntityGuid2, result2.Value.ReadModel.Id);
            runner.Dispose();
        }

        [TestMethod]
        public void CreatedOnAttribute_Exception()
        {
            Assert.ThrowsException<ArgumentException>(() => new CreateReadmodelOnAttribute(typeof(ReadModel)));
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

    [CreateReadmodelOn(typeof(TestEvnt2))]
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

    [CreateReadmodelOn(typeof(TestEvnt2))]
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

    [CreateReadmodelOn(typeof(TestEvnt1))]
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

    [CreateReadmodelOn(typeof(TestEvnt2))]
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