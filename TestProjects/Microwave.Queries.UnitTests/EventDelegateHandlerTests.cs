using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Domain.EventSourcing;
using Microwave.Domain.Identities;
using Microwave.Persistence.MongoDb.Querries;
using Microwave.Persistence.UnitTestsSetup.MongoDb;
using Microwave.Queries.Handler;
using Microwave.Queries.Ports;
using Moq;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class EventDelegateHandlerTests : IntegrationTests
    {
        private Mock<IVersionRepository> _versionRepo;
        private Mock<IRemoteVersionRepository> _remoteVersionRepo;

        [TestInitialize]
        public void Setup()
        {
            _versionRepo = new Mock<IVersionRepository>();
            _versionRepo.Setup(repo => repo.SaveVersionAsync(It.IsAny<LastProcessedVersion>())).Returns(Task.CompletedTask);
            _versionRepo.Setup(repo => repo.GetVersionAsync(It.IsAny<string>())).ReturnsAsync(DateTimeOffset.MinValue);

            _remoteVersionRepo = new Mock<IRemoteVersionRepository>();
            _remoteVersionRepo.Setup(repo => repo.SaveVersionAsync(It.IsAny<LastProcessedVersion>())).Returns(Task.CompletedTask);
            _remoteVersionRepo.Setup(repo => repo.GetVersionAsync(It.IsAny<string>())).ReturnsAsync(DateTimeOffset.MinValue);
        }

        [TestMethod]
        public async Task MixedEventsInFeed()
        {
            var mock = new Mock<IEventFeed<AsyncEventHandler<TestEv>>>();
            IEnumerable<SubscribedDomainEventWrapper> list = new [] {
                new SubscribedDomainEventWrapper
                {
                    DomainEvent = new TestEv(GuidIdentity.Create(Guid.NewGuid()))
                },
                new SubscribedDomainEventWrapper
                {
                    DomainEvent = new TestEv2(GuidIdentity.Create(Guid.NewGuid()))
                }
            };

            mock.Setup(feed => feed.GetEventsAsync(It.IsAny<DateTimeOffset>())).ReturnsAsync(list);
            var handler = new Handler();
            var eventDelegateHandler = new AsyncEventHandler<TestEv>(
                _versionRepo.Object,
                _remoteVersionRepo.Object,
                mock.Object,
                handler);
            await eventDelegateHandler.Update();
            Assert.AreEqual(1, handler.WasCalled);
        }

        [TestMethod]
        public async Task MixedEventsInFeed_QuerryRepo()
        {
            var mock = new Mock<IEventFeed<QueryEventHandler<TestQ, TestEv>>>();
            IEnumerable<SubscribedDomainEventWrapper> list = new [] { new SubscribedDomainEventWrapper
                {
                    DomainEvent = new TestEv(GuidIdentity.Create(Guid.NewGuid()))
                },
                new SubscribedDomainEventWrapper
                {
                    DomainEvent = new TestEv2(GuidIdentity.Create(Guid.NewGuid()))
                }
            };
            mock.Setup(feed => feed.GetEventsAsync(It.IsAny<DateTimeOffset>())).ReturnsAsync(list);

            var queryRepository = new ReadModelRepositoryMongoDb(EventMongoDb);

            var eventDelegateHandler = new QueryEventHandler<TestQ, TestEv>(
                queryRepository,
                _versionRepo.Object,
                mock.Object,
                _remoteVersionRepo.Object);
            await eventDelegateHandler.Update();

            var result = await queryRepository.LoadAsync<TestQ>();
            Assert.AreEqual(1, result.Value.WasCalled);
        }
    }

    public class TestQ : Query, IHandle<TestEv>
    {
        public void Handle(TestEv domainEvent)
        {
            WasCalled++;
        }

        public int WasCalled { get; set; }
    }

    public class Handler : IHandleAsync<TestEv>
    {
        public Task HandleAsync(TestEv domainEvent)
        {
            WasCalled++;
            return Task.CompletedTask;
        }

        public int WasCalled { get; set; }
    }

    public class TestEv : IDomainEvent, ISubscribedDomainEvent
    {
        public TestEv(GuidIdentity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }

    public class TestEv2 : IDomainEvent, ISubscribedDomainEvent
    {
        public TestEv2(GuidIdentity entityId)
        {
            EntityId = entityId;
        }

        public Identity EntityId { get; }
    }
}