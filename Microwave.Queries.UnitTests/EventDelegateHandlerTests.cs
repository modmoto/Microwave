using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microwave.Application;
using Microwave.Domain;
using Microwave.ObjectPersistences;
using Moq;

namespace Microwave.Queries.UnitTests
{
    [TestClass]
    public class EventDelegateHandlerTests
    {
        [TestMethod]
        public async Task MixedEventsInFeed()
        {
            var mock = new Mock<IEventFeed<TestEv>>();
            IEnumerable<DomainEventWrapper> list = new [] { new DomainEventWrapper
            {
                DomainEvent = new TestEv(Guid.NewGuid())
            },
                new DomainEventWrapper
                {
                    DomainEvent = new TestEv2(Guid.NewGuid())
                }
            };
            mock.Setup(feed => feed.GetEventsAsync(It.IsAny<long>())).ReturnsAsync(list);
            var versionRepo = new Mock<IVersionRepository>();
            versionRepo.Setup(repo => repo.SaveVersion(It.IsAny<LastProcessedVersion>())).Returns(Task.CompletedTask);
            versionRepo.Setup(repo => repo.GetVersionAsync(It.IsAny<string>())).ReturnsAsync(0);
            var handler = new Handler();
            var eventDelegateHandler = new EventDelegateHandler<TestEv>(versionRepo.Object, mock.Object, new []{ handler });
            await eventDelegateHandler.Update();
            Assert.AreEqual(1, handler.WasCalled);
        }

        [TestMethod]
        public async Task MixedEventsInFeed_QuerryRepo()
        {
            var mock = new Mock<IEventFeed<TestEv>>();
            IEnumerable<DomainEventWrapper> list = new [] { new DomainEventWrapper
                {
                    DomainEvent = new TestEv(Guid.NewGuid())
                },
                new DomainEventWrapper
                {
                    DomainEvent = new TestEv2(Guid.NewGuid())
                }
            };
            mock.Setup(feed => feed.GetEventsAsync(It.IsAny<long>())).ReturnsAsync(list);
            var versionRepo = new Mock<IVersionRepository>();
            versionRepo.Setup(repo => repo.SaveVersion(It.IsAny<LastProcessedVersion>())).Returns(Task.CompletedTask);
            versionRepo.Setup(repo => repo.GetVersionAsync(It.IsAny<string>())).ReturnsAsync(0);

            var options = new DbContextOptionsBuilder<QueryStorageContext>()
                .UseInMemoryDatabase("IdentifiableQuerySaveAndLoad")
                .Options;

            var queryRepository = new QueryRepository(new QueryStorageContext(options), new ObjectConverter());

            var eventDelegateHandler = new QueryEventHandler<TestQ, TestEv>(queryRepository, versionRepo.Object, mock.Object);
            await eventDelegateHandler.Update();

            var result = await queryRepository.Load<TestQ>();
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

    public class TestEv : IDomainEvent
    {
        public TestEv(Guid entityId)
        {
            EntityId = entityId;
        }

        public Guid EntityId { get; }
    }

    public class TestEv2 : IDomainEvent
    {
        public TestEv2(Guid entityId)
        {
            EntityId = entityId;
        }

        public Guid EntityId { get; }
    }
}